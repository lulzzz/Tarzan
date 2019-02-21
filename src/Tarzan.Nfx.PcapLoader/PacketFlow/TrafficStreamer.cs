using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Deployment;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Model.Observable;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader.PacketFlow
{
    [Serializable]
    public partial class TrafficStreamer : PcapProcessor
    {
        public override async Task Invoke()
        {
            var cfg = GetIgniteConfiguration();

            cfg.PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain;
            cfg.ClientMode = true;
            
            using (var ignite = Ignition.Start(cfg))
            {
                ignite.GetCompute().Broadcast(new NotificationAction("Start streaming..."));
                foreach (var fileInfo in SourceFiles)
                {
                    OnFileOpened(fileInfo);
                    using (var device = new FastPcapFileReaderDevice(fileInfo.FullName))
                    {
                        await ProcessFile(ignite, fileInfo, device);
                    }
                    OnFileCompleted(fileInfo);
                }
                ignite.GetCompute().Broadcast(new NotificationAction("Done."));
            }
        }

        private async Task ProcessFile(IIgnite client, FileInfo fileInfo, FastPcapFileReaderDevice device)
        {
            device.Open();
            var frameKeyProvider = new FrameKeyProvider();
            var cache = CacheFactory.GetOrCreateCache<string, Artifact>(client, FrameCacheName ?? fileInfo.Name);
            var flowTracker = new PacketFlowTracker(new FrameKeyProvider());

            using (var dataStreamer = client.GetDataStreamer<string, Artifact>(cache.Name))
            {
                dataStreamer.AllowOverwrite = true;
                dataStreamer.Receiver = new PacketFlowVisitor(new MergePacketFlowProcessor());

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
                    flowTracker.ProcessFrame(frame);
                    if (flowTracker.TotalFrameCount == ChunkSize)
                    {
                        OnChunkLoaded(currentChunkNumber, currentChunkBytes);
                        cacheStoreTask = cacheStoreTask.ContinueWith(StreamData(dataStreamer, flowTracker.FlowTable, currentChunkNumber, currentChunkBytes));
                        flowTracker.Reset();
                    }
                
                }
              
                OnChunkLoaded(currentChunkNumber, currentChunkBytes);
                cacheStoreTask = cacheStoreTask.ContinueWith(StreamData(dataStreamer, flowTracker.FlowTable, currentChunkNumber, currentChunkBytes));

                await cacheStoreTask;
             
                dataStreamer.Flush();
            }
            Console.WriteLine($"Stored flows: {cache.GetSize()}");
            device.Close();
        }

        /// <summary>
        /// Creates an action that streams data in the cache.
        /// </summary>
        /// <param name="dataStreamer"></param>
        /// <param name="list"></param>
        /// <param name="currentChunkNumber"></param>
        /// <param name="currentChunkBytes"></param>
        /// <returns></returns>
        private Action<Task> StreamData(IDataStreamer<string, Artifact> dataStreamer, IEnumerable<KeyValuePair<FlowKey, IList<FrameData>>> list, int currentChunkNumber, int currentChunkBytes)
        {
            var items = list.Select(x => KeyValuePair.Create(x.Key.ToString(), new Artifact { PayloadBin = x.Value.SelectMany(y => y.GetBytes()).ToArray() })).ToList();
            return async (t) => {
                    await dataStreamer.AddData(items);
                    this.OnChunkStored(currentChunkNumber, currentChunkBytes);
            };
        }

        class NotificationAction : IComputeAction
        {
            private readonly string m_message;

            public NotificationAction(string message)
            {
                m_message = message;
            }
            public void Invoke()
            {
                Console.WriteLine($"{m_message}");
            }
        }
    }
}
