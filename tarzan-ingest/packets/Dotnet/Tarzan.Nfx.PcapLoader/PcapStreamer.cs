using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.Logging.Console;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    public class PcapStreamer : IPcapProcessor
    {
        const int maxOnHeap = 1024;
        const int maxOffHeap = 1024;
        public const int DEFAULT_PORT = 47500;
        public const int CHUNK_SIZE = 100;
        readonly ConsoleLogger m_logger = new ConsoleLogger("Loader", (s, ll) => true, true);

        public int ChunkSize { get; set; } = CHUNK_SIZE;

        public IList<FileInfo> SourceFiles { get; private set; } = new List<FileInfo>();

        public IPEndPoint ClusterNode { get; set; } = new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT);

        public event FileOpenHandler OnFileOpen;
        public event FileCompletedHandler OnFileCompleted;
        public event ChunkCompletedHandler OnChunkLoaded;
        public event ChunkCompletedHandler OnChunkStored;
        public event ErrorFrameHandler OnErrorFrame;

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
                },
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
            var cache = client.GetOrCreateCache<FrameKey, Frame>(fileInfo.Name);
            using (var dataStreamer = client.GetDataStreamer<FrameKey, Frame>(cache.Name))
            {
                var frameIndex = 0;
                var frameArray = new KeyValuePair<FrameKey, Frame>[ChunkSize];
                var cacheStoreTask = Task.CompletedTask;

                var currentChunkBytes = 0;
                var currentChunkNumber = 0;
                RawCapture rawCapture = null;
                while ((rawCapture = device.GetNextPacket()) != null)
                {
                    currentChunkBytes += rawCapture.Data.Length + 4 * sizeof(int);

                    var frame = new Frame
                    {
                        LinkLayer = (LinkLayerType)rawCapture.LinkLayerType,
                        Timestamp = rawCapture.Timeval.ToUnixTimeMilliseconds(),
                        Data = rawCapture.Data
                    };
                    var frameKey = new FrameKey { FrameNumber = frameIndex, FlowKeyHash = frameKeyProvider.GetKeyHash(frame) };

                    frameArray[frameIndex % ChunkSize] = KeyValuePair.Create(frameKey, frame);

                    // Is CHUNK full?
                    if (frameIndex % ChunkSize == ChunkSize - 1)
                    {
                        OnChunkLoaded?.Invoke(this, currentChunkNumber, currentChunkBytes);
                        cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(dataStreamer, frameArray, ChunkSize, currentChunkNumber, currentChunkBytes));
                        frameArray = new KeyValuePair<FrameKey, Frame>[ChunkSize];
                        currentChunkNumber++;
                        currentChunkBytes = 0;
                    }
                    frameIndex++;
                }

                OnChunkLoaded?.Invoke(this, currentChunkNumber, currentChunkBytes);

                cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(dataStreamer, frameArray, frameIndex % ChunkSize, currentChunkNumber, currentChunkBytes));

                await cacheStoreTask;
                dataStreamer.Flush();
                //dataStreamer.Close(false);   // HACK: causes Exception in JVM.DLL
            }
            device.Close();
        }

        private Action<Task> CreateStoreAction(IDataStreamer<FrameKey, Frame> dataStreamer, ICollection<KeyValuePair<FrameKey, Frame>> frameArray, int count, int currentChunkNumber, int currentChunkBytes)
        {
            return async (t) => await StoreChunk(dataStreamer, frameArray, count, currentChunkNumber, currentChunkBytes);
        }

        private async Task StoreChunk(IDataStreamer<FrameKey, Frame> dataStreamer, ICollection<KeyValuePair<FrameKey, Frame>> frameArray, int count, int currentChunkNumber, int currentChunkBytes)
        {
            await dataStreamer.AddData(frameArray);
            OnChunkStored?.Invoke(this, currentChunkNumber, currentChunkBytes);
        }
    }
}
