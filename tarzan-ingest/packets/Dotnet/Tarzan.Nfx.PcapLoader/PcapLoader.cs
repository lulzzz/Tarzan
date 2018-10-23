using Apache.Ignite.Core;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Compute;
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
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

using FrameCache = Apache.Ignite.Core.Client.Cache.ICacheClient<Tarzan.Nfx.Model.FrameKey, Tarzan.Nfx.Model.Frame>;

namespace Tarzan.Nfx.PcapLoader
{
    public class PcapLoader : IPcapProcessor
    {
        public const int DEFAULT_PORT = 10800;
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
            var cfg = new IgniteClientConfiguration
            {
                Host = ClusterNode.Address.ToString(),
                Port = ClusterNode.Port !=0 ? ClusterNode.Port : DEFAULT_PORT
            };

            using (var client = Ignition.StartClient(cfg))
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

        private async Task ProcessFile(IIgniteClient client, FileInfo fileInfo, FastPcapFileReaderDevice device)
        {
            device.Open();

            var frameKeyProvider = new FrameKeyProvider();
            var packetCache = client.GetOrCreateCache<FrameKey, Frame>(fileInfo.Name);

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
                    cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(packetCache, frameArray, ChunkSize, currentChunkNumber, currentChunkBytes));
                    frameArray = new KeyValuePair<FrameKey, Frame>[ChunkSize];
                    currentChunkNumber++;
                    currentChunkBytes = 0;
                }
                frameIndex++;
            }

            OnChunkLoaded?.Invoke(this, currentChunkNumber, currentChunkBytes);
            cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(packetCache, frameArray, frameIndex % ChunkSize, currentChunkNumber, currentChunkBytes));

            await cacheStoreTask;
            device.Close();
        }

        private Action<Task> CreateStoreAction(FrameCache packetCache, KeyValuePair<FrameKey, Frame>[] frameArray, int count, int currentChunkNumber, int currentChunkBytes)
        {
            return (t) => StoreChunk(packetCache, frameArray, count, currentChunkNumber, currentChunkBytes);
        }

        private void StoreChunk(FrameCache packetCache, KeyValuePair<FrameKey, Frame>[] frameArray, int count, int currentChunkNumber, int currentChunkBytes)
        {
            packetCache.PutAll(frameArray.Take(count));
            OnChunkStored?.Invoke(this, currentChunkNumber, currentChunkBytes);
        }
    }
}
