using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Resource;
using Netdx.PacketDecoders;
using SharpPcap;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tarzan.Nfx.Ingest.Flow;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    class FlowAnalyzer : IComputeAction
    {
        public string FileName { get; set; }

        [InstanceResource]
        private readonly IIgnite m_ignite;

        public void Invoke()
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Start processing file '{FileName}'...");

            var device = new FastPcapFileReaderDevice(FileName);
            device.Open();

            var sw = new Stopwatch();
            sw.Start();

            var flowTracker = new FlowTracker(new FrameKeyProvider());
            RawCapture packet = null;
            while ((packet = device.GetNextPacket())!=null)
            {
                var frame = new Frame((LinkLayerType)packet.LinkLayerType, PosixTime.FromUnixTimeMilliseconds(packet.Timeval.ToUnixTimeMilliseconds()), packet.Data);
                flowTracker.ProcessFrame(frame);
            }
            
            device.Close();
           
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Done ({sw.Elapsed}), packets={flowTracker.TotalFrameCount}, flows={flowTracker.FlowTable.Count}.");

            sw.Restart();

            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Streaming to global FLOW CACHE...");

            var globalFlowTable = m_ignite.GetOrCreateCache<FlowKey, PacketStream>(IgniteConfiguration.FlowCache);
            var updateProcessor = new MergePacketStreamProcessor();
            Parallel.ForEach(flowTracker.FlowTable, flow =>
                {
                    globalFlowTable.Invoke(flow.Key, updateProcessor, flow.Value);
                });

            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Done ({sw.Elapsed}).");
            sw.Stop();
        }
    }
}

