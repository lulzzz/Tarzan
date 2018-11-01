using Apache.Ignite.Core;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    internal class PcapVerifier : IPcapProcessor
    {
        public const int DEFAULT_PORT = 47500;
        public const int CHUNK_SIZE = 100;

        public int ChunkSize { get; set; } = CHUNK_SIZE;

        public IPEndPoint ClusterNode { get; set; } = new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT);
        public IList<FileInfo> SourceFiles { get; private set; } = new List<FileInfo>();
        public string FrameCacheName { get; set; } = null;

        public event ChunkCompletedHandler OnChunkLoaded;
        public event ChunkCompletedHandler OnChunkStored;
        public event FileCompletedHandler OnFileCompleted;
        public event FileOpenHandler OnFileOpen;
        public event ErrorFrameHandler OnErrorFrame;

        const int maxOnHeap = 1024;
        const int maxOffHeap = 1024;

        public async Task Invoke()
        {
            var cfg = new IgniteConfiguration
            {
                JvmOptions = new[] { //"-Xms256m",
                                     $"-Xmx{maxOnHeap}m",
                                     "-XX:+AlwaysPreTouch",
                                     "-XX:+UseG1GC",
                                     "-XX:+ScavengeBeforeFullGC",
                                     "-XX:+DisableExplicitGC",
                                     $"-XX:MaxDirectMemorySize={maxOffHeap}m" },
                DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                {

                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        Endpoints = new[] { $"{ClusterNode.Address}:{(ClusterNode.Port != 0 ? ClusterNode.Port : DEFAULT_PORT)}" }
                    },
                }
            };
            Ignition.ClientMode = true;
            using (var client = Ignition.Start(cfg))
            {
                foreach (var fileInfo in SourceFiles)
                {
                    OnFileOpen?.Invoke(this, fileInfo);
                    using (var device = new FastPcapFileReaderDevice(fileInfo.FullName))
                    {
                        await ProcessFile(client, fileInfo, device);
                    }
                    OnFileCompleted?.Invoke(this, fileInfo);
                }
            }
        }
        private async Task ProcessFile(IIgnite client, FileInfo fileInfo, FastPcapFileReaderDevice device)
        {
            device.Open();
            var frameKeyProvider = new FrameKeyProvider();
            var cache = CacheFactory.GetOrCreateFrameCache(client, FrameCacheName ?? fileInfo.Name);
            RawCapture rawCapture = null;
            var frameIndex = 0;
            var currentChunkBytes = 0;
            var currentChunkNumber = 0;
            while ((rawCapture = device.GetNextPacket()) != null)
            {
                currentChunkBytes += rawCapture.Data.Length + 4 * sizeof(int);

                var frame = new FrameData
                {
                    LinkLayer = (LinkLayerType)rawCapture.LinkLayerType,
                    Timestamp = rawCapture.Timeval.ToUnixTimeMilliseconds(),
                    Data = rawCapture.Data
                };
                var frameKey = new FrameKey { FrameNumber = frameIndex, FlowKeyHash = frameKeyProvider.GetKeyHash(frame) };
                var storedFrame = cache.Get(frameKey);
                if (storedFrame == null) OnErrorFrame?.Invoke(this, fileInfo, frameIndex, null);
                if (storedFrame != null && frame.Timestamp != storedFrame.Timestamp) OnErrorFrame?.Invoke(this, fileInfo, frameIndex, storedFrame);

                if (frameIndex % ChunkSize == ChunkSize - 1)
                {
                    OnChunkLoaded?.Invoke(this, currentChunkNumber, currentChunkBytes);
                    OnChunkStored?.Invoke(this, currentChunkNumber, currentChunkBytes);
                    currentChunkNumber++;
                    currentChunkBytes = 0;
                }
                frameIndex++;
            }
            device.Close();
        }
    }
}