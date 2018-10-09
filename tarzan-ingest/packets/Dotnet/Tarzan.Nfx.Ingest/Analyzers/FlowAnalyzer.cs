using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Netdx.PacketDecoders;
using SharpPcap;
using System;
using System.Diagnostics;
using Tarzan.Nfx.FlowTracker;
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

        private IFlowTracker<FlowRecord> TrackFlows()
        {
            var sw = new Stopwatch();
            sw.Start();

            var flowTracker = new FlowWithContentTracker(new FrameKeyProvider());
            using (var device = new FastPcapFileReaderDevice(FileName))
            {
                device.Open();

                RawCapture packet = null;
                while ((packet = device.GetNextPacket()) != null)
                {
                    try
                    {
                        var frame = new Frame
                        {
                            LinkLayer = (LinkLayerType)packet.LinkLayerType,
                            Timestamp = packet.Timeval.ToUnixTimeMilliseconds(),
                            Data = packet.Data
                        };
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

        /// <summary>
        /// Loads a local flow cache to the global flow table.
        /// </summary>
        /// <param name="flowTracker">A flow tracker object that contains a local flow cache.</param>
        private void PopulateFlowTable(IFlowTracker<FlowRecord> flowTracker)
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]   INGEST: FlowAnalyzer: Streaming to global FLOW CACHE...");
            var sw = new Stopwatch();
            sw.Start();
           
            var globalFlowTable = new PacketFlowTable(m_ignite);
            var flowCache = globalFlowTable.GetOrCreateCache();
            var updateProcessor = new MergePacketFlowProcessor();

            foreach (var flow in flowTracker.FlowTable)
            {
                flow.Value.Flow.FlowUid = FlowUidGenerator.NewUid(flow.Key, flow.Value.Flow.FirstSeen);
                flowCache.Invoke(flow.Key, updateProcessor, flow.Value.Flow);
            }

            sw.Stop();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]   INGEST: FlowAnalyzer: Done ({sw.Elapsed}).");
        }
    }
}

