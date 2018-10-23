using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.Analyzers
{


    [Serializable]
    public partial class FlowAnalyzer : IComputeAction
    {
        public class ProgressRecord
        {
            public int CompletedFrames { get; set; }
            public int TotalFrames { get; set; }
            public int CompletedFlows { get; set; }
            public int TotalFlows { get; set; }
            public Stopwatch ElapsedTime { get; set; }
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
            var progress = new ProgressRecord() { ElapsedTime = new Stopwatch() };
            progress.ElapsedTime.Start();
            var flowTracker = TrackFlows(progress);
            PopulateFlowTable(flowTracker, progress);
            progress.ElapsedTime.Stop();
        }

        private IFlowTracker<PacketFlow> TrackFlows(ProgressRecord progressRecord)
        {
            var cache = m_ignite.GetCache<FrameKey, Frame>(CacheName);
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
            var flowCache = m_ignite.GetOrCreateCache<FlowKey, PacketFlow>(PacketFlow.CACHE_NAME);
            using (var dataStreamer = m_ignite.GetDataStreamer<FlowKey, PacketFlow>(flowCache.Name))
            {
                dataStreamer.AllowOverwrite = true;
                var updateProcessor = new MergePacketFlowProcessor();
                dataStreamer.Receiver = new FlowStreamVisitor(updateProcessor);
                
                progressRecord.TotalFlows = flowTracker.FlowTable.Count;
                var flowCount = 0;
                foreach (var flow in flowTracker.FlowTable)
                {
                    flow.Value.FlowUid = FlowUidGenerator.NewUid(flow.Key, flow.Value.FirstSeen);
                    dataStreamer.AddData(flow.Key, flow.Value);
                    if (++flowCount % ProgressFlowBatch == 0)
                    {
                        progressRecord.CompletedFlows += ProgressFlowBatch;
                        Progress?.Report(progressRecord);
                    }
                }
                progressRecord.CompletedFlows += flowCount % ProgressFlowBatch;
                Progress?.Report(progressRecord);
                dataStreamer.Flush();
            }
        }
        [Serializable]
        public sealed class FlowStreamVisitor : IStreamReceiver<FlowKey, PacketFlow>
        {
            private MergePacketFlowProcessor updateProcessor;
            public FlowStreamVisitor(MergePacketFlowProcessor updateProcessor)
            {
                this.updateProcessor = updateProcessor;
            }
            public void Receive(ICache<FlowKey, PacketFlow> cache, ICollection<ICacheEntry<FlowKey, PacketFlow>> entries)
            {
                foreach (var entry in entries)
                {
                    cache.Invoke(entry.Key, updateProcessor, entry.Value);
                }
            }
        }
    }
}

