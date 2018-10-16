using Apache.Ignite.Core;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.Logging.Console;
using SharpPcap;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    public delegate void FileOpenHandler(object sender, FileInfo fileInfo);
    public delegate void FileCompletedHandler(object sender, FileInfo fileInfo);
    public delegate void ChunkCompletedHandler(object sender, int chunkNumber, int chunkBytes);

    public class PcapStreamer : IPcapProcessor
    {
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

        public async Task Invoke()
        {
            var cfg = new IgniteConfiguration
            {
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

            var cahce = client.GetOrCreateCache<int, Frame>(fileInfo.Name);
            var dataStreamer = client.GetDataStreamer<int, Frame>(fileInfo.Name);
            //dataStreamer.PerNodeBufferSize = 8192;
            //dataStreamer.PerNodeParallelOperations = 4;
            var frameIndex = 0;
            var frameArray = new KeyValuePair<int, Frame>[ChunkSize];
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

                frameArray[frameIndex % ChunkSize] = KeyValuePair.Create(frameIndex, frame);

                // Is CHUNK full?
                if (frameIndex % ChunkSize == ChunkSize - 1)
                {
                    OnChunkLoaded?.Invoke(this, currentChunkNumber, currentChunkBytes);
                    cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(dataStreamer, frameArray, ChunkSize, currentChunkNumber, currentChunkBytes));
                    frameArray = new KeyValuePair<int, Frame>[ChunkSize];
                    currentChunkNumber++;
                    currentChunkBytes = 0;
                }
                frameIndex++;
            }

            OnChunkLoaded?.Invoke(this, currentChunkNumber, currentChunkBytes);
            cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(dataStreamer, frameArray, frameIndex % ChunkSize, currentChunkNumber, currentChunkBytes));

            await cacheStoreTask;
            device.Close();
        }

        private Action<Task> CreateStoreAction(IDataStreamer<int, Frame> dataStreamer, KeyValuePair<int, Frame>[] frameArray, int count, int currentChunkNumber, int currentChunkBytes)
        {
            return (t) => StoreChunk(dataStreamer, frameArray, count, currentChunkNumber, currentChunkBytes);
        }

        private void StoreChunk(IDataStreamer<int, Frame> dataStreamer, KeyValuePair<int, Frame>[] frameArray, int count, int currentChunkNumber, int currentChunkBytes)
        {
            for(int i=0;i<count;i++)
            {
                dataStreamer.AddData(frameArray[i]);
            }

            OnChunkStored?.Invoke(this, currentChunkNumber, currentChunkBytes);
        }
    }
}
