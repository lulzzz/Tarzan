using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.NLog;
using Microsoft.Extensions.Logging.Console;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    [Serializable]
    public class PcapStreamer : PcapProcessor
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        readonly ConsoleLogger m_logger = new ConsoleLogger("Loader", (s, ll) => true, true);
        
        public override async Task Invoke()
        {
            Ignition.ClientMode = true;
            var cfg = GetIgniteConfiguration();
            cfg.Logger = new IgniteNLogLogger();
            using (var client = Ignition.Start(cfg))
            {
                foreach (var fileInfo in SourceFiles)
                {
                    OnFileOpened(fileInfo);
                    using (var device = new FastPcapFileReaderDevice(fileInfo.FullName))
                    {
                        await ProcessFile(client, fileInfo, device);
                    }
                    OnFileCompleted(fileInfo);
                }
            }
        }

        private async Task ProcessFile(IIgnite client, FileInfo fileInfo, FastPcapFileReaderDevice device)
        {
            device.Open();
            var frameKeyProvider = new FrameKeyProvider();
            var cache = CacheFactory.GetOrCreateFrameCache(client, FrameCacheName??fileInfo.Name);
            using (var dataStreamer = client.GetDataStreamer<FrameKey, FrameData>(cache.Name))
            {
                var frameIndex = 0;
                var frameArray = new KeyValuePair<FrameKey, FrameData>[ChunkSize];
                var cacheStoreTask = Task.CompletedTask;

                var currentChunkBytes = 0;
                var currentChunkNumber = 0;
                RawCapture rawCapture = null;
                while ((rawCapture = device.GetNextPacket()) != null)
                {
                    currentChunkBytes += rawCapture.Data.Length + 4 * sizeof(int);

                    var frame = new FrameData
                    {
                        LinkLayer = (LinkLayerType)rawCapture.LinkLayerType,
                        Timestamp = rawCapture.Timeval.ToUnixTimeMilliseconds(),
                        Data = rawCapture.Data
                    };
                    var frameKey = new FrameKey(frameIndex, frameKeyProvider.GetKeyHash(frame));

                    frameArray[frameIndex % ChunkSize] = KeyValuePair.Create(frameKey, frame);

                    // Is CHUNK full?
                    if (frameIndex % ChunkSize == ChunkSize - 1)
                    {
                        OnChunkLoaded(currentChunkNumber, currentChunkBytes);
                        cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(dataStreamer, frameArray, ChunkSize, currentChunkNumber, currentChunkBytes));
                        currentChunkNumber++;
                        currentChunkBytes = 0;
                    }
                    frameIndex++;
                }

                OnChunkLoaded(currentChunkNumber, currentChunkBytes);

                cacheStoreTask = cacheStoreTask.ContinueWith(CreateStoreAction(dataStreamer, frameArray, frameIndex % ChunkSize, currentChunkNumber, currentChunkBytes));

                await cacheStoreTask;
                dataStreamer.Flush();
                //dataStreamer.Close(false);   // HACK: causes Exception in JVM.DLL
            }
            device.Close();
        }

        private Action<Task> CreateStoreAction(IDataStreamer<FrameKey, FrameData> dataStreamer, KeyValuePair<FrameKey, FrameData>[] frameArray, int count, int currentChunkNumber, int currentChunkBytes)
        {
            var buffer = new KeyValuePair<FrameKey, FrameData>[count];
            Array.Copy(frameArray, buffer, count);
            return async (t) => await StoreChunk(dataStreamer, buffer, currentChunkNumber, currentChunkBytes);
        }

        private async Task StoreChunk(IDataStreamer<FrameKey, FrameData> dataStreamer, KeyValuePair<FrameKey, FrameData>[] frameArray, int currentChunkNumber, int currentChunkBytes)
        {
            await dataStreamer.AddData(frameArray);
            this.OnChunkStored(currentChunkNumber, currentChunkBytes);
        }
    }
}
