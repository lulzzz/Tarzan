using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.FlowTracker
{


    [Serializable]
    class FlowAnalyzer : IComputeAction
    {
        public class ProgressRecord
        {
            public int CompletedFrames { get; set; }
            public int TotalFrames { get; set; }
            public int CompletedFlows { get; set; }
            public int TotalFlows { get; set; }
        }

        public IProgress<ProgressRecord> Progress { get; set; } = null;
        public int ProgressFrameBatch { get; set; } = 1000;
        public int ProgressFlowBatch { get; set; } = 100;
        public string CacheName { get; set; }

        [InstanceResource]
        private readonly IIgnite m_ignite;

        public FlowAnalyzer(IIgnite ignite=null)
        {
            if (ignite != null) m_ignite = ignite;
        }

        public void Invoke()
        {
            var progress = new ProgressRecord();
            var flowTracker = TrackFlows(progress);
            PopulateFlowTable(flowTracker, progress);
        }

        private IFlowTracker<PacketFlow> TrackFlows(ProgressRecord progressRecord)
        {
            var cache = m_ignite.GetCache<int, Frame>(CacheName);
            progressRecord.TotalFrames = cache.GetLocalSize();
            Progress?.Report(progressRecord);
            var flowTracker = new FlowTracker(new FrameKeyProvider());
            
            var framesCount = 0;
            foreach (var frame in cache.GetLocalEntries())
            {
                flowTracker.ProcessFrame(frame.Value);
                if (++framesCount % ProgressFrameBatch == 0)
                {
                    progressRecord.CompletedFrames += ProgressFrameBatch;
                    Progress?.Report(progressRecord);
                }
            }
            progressRecord.CompletedFrames += framesCount % ProgressFrameBatch;
            Progress?.Report(progressRecord);
            return flowTracker;
        }

        /// <summary>
        /// Loads a local flow cache to the global flow table.
        /// </summary>
        /// <param name="flowTracker">A flow tracker object that contains a local flow cache.</param>
        private void PopulateFlowTable(IFlowTracker<PacketFlow> flowTracker, ProgressRecord progressRecord)
        {
            var flowCache = m_ignite.GetOrCreateCache<FlowKey, PacketFlow>("flowtable");
            var updateProcessor = new MergePacketFlowProcessor();
            progressRecord.TotalFlows = flowTracker.FlowTable.Count;

            var flowCount = 0;
            foreach (var flow in flowTracker.FlowTable)
            {
                flow.Value.FlowUid = FlowUidGenerator.NewUid(flow.Key, flow.Value.FirstSeen);
                flowCache.Invoke(flow.Key, updateProcessor, flow.Value);
                if (++flowCount % ProgressFlowBatch == 0)
                {
                    progressRecord.CompletedFlows += ProgressFlowBatch;
                    Progress?.Report(progressRecord);
                }
            }
            progressRecord.CompletedFlows += flowCount % ProgressFlowBatch;
            Progress?.Report(progressRecord);
        }
    }
}

