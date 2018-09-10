using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Netdx.PacketDecoders;
using SharpPcap;
using System;
using System.Diagnostics;
using Tarzan.Nfx.Ingest.Flow;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Ingest.Utils;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    class FlowAnalyzer : IComputeAction
    {
        public string FileName { get; set; }

        [InstanceResource]
        private readonly IIgnite m_ignite;

        public void Invoke()
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]   INGEST: FlowAnalyzer: Start processing file '{FileName}'...");

            var flowTracker = TrackFlows();

            PopulateFlowTable(flowTracker);
        }

        private FlowTracker TrackFlows()
        {
            var sw = new Stopwatch();
            sw.Start();

            var flowTracker = new FlowTracker(new FrameKeyProvider());
            using (var device = new FastPcapFileReaderDevice(FileName))
            {
                device.Open();

                RawCapture packet = null;
                while ((packet = device.GetNextPacket()) != null)
                {
                    try
                    {
                        var frame = new Frame((LinkLayerType)packet.LinkLayerType, PosixTime.FromUnixTimeMilliseconds(packet.Timeval.ToUnixTimeMilliseconds()), packet.Data);
                        flowTracker.ProcessFrame(frame);
                    }
                    catch(Exception e)
                    {
                        // TODO: Log any error occured here.
                    }
                }

                device.Close();
            }
            sw.Stop();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]   INGEST: FlowAnalyzer: Done ({sw.Elapsed}), packets={flowTracker.TotalFrameCount}, flows={flowTracker.FlowTable.Count}.");

            return flowTracker;
        }

        private void PopulateFlowTable(FlowTracker flowTracker)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]   INGEST: FlowAnalyzer: Streaming to global FLOW CACHE...");
            var sw = new Stopwatch();
            sw.Start();
           
            var globalFlowTable = new PacketFlowTable(m_ignite);
            var flowCache = globalFlowTable.GetOrCreateCache();
            var updateProcessor = new MergePacketFlowProcessor();

            foreach (var flow in flowTracker.FlowTable)
            {
                flowCache.Invoke(flow.Key, updateProcessor, flow.Value.flow);
            }

            sw.Stop();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]   INGEST: FlowAnalyzer: Done ({sw.Elapsed}).");
        }
    }
}

