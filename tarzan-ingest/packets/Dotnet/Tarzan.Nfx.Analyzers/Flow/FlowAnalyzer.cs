using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.Analyzers
{
    /// <summary>
    /// Takes all local objects from the frame cache specified by <see cref="FrameCacheName"/>, computes flow objects and 
    /// merges the flow objects to the flow cache specified by <see cref="FlowCacheName"/>.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the name of the frame cache;
        /// </summary>
        public string FrameCacheName { get; set; }
        /// <summary>
        /// Gets or sets the name of the flow cache.
        /// </summary>
        public string FlowCacheName { get; set; }

        [InstanceResource]
        private readonly IIgnite m_ignite;

        public FlowAnalyzer(IIgnite ignite=null)
        {
            if (ignite != null) m_ignite = ignite;
        }

        /// <summary>
        /// Invokes the computation of the current compute action.
        /// </summary>
        public void Invoke()
        {
            Console.WriteLine("FlowAnalyzer is running...");
            var progress = new ProgressRecord() { ElapsedTime = new Stopwatch() };
            progress.ElapsedTime.Start();
            Console.WriteLine($"Tracking flows in {FrameCacheName}...");
            var flowTracker = TrackFlows(progress);
            Console.WriteLine($"Writing flows to {FlowCacheName}...");
            PopulateFlowTable(flowTracker, progress);                            
            progress.ElapsedTime.Stop();
            Console.WriteLine($"FlowAnalyzer completed, time elapsed={progress.ElapsedTime.ElapsedMilliseconds}ms.");
        }

        private IFlowTracker<FlowData> TrackFlows(ProgressRecord progressRecord)
        {
            var flowTracker = new FlowTracker(new FrameKeyProvider());
            try
            {
                var cache = CacheFactory.GetOrCreateFrameCache(m_ignite, FrameCacheName);
                progressRecord.TotalFrames = cache.GetLocalSize();
                Progress?.Report(progressRecord);

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
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            return flowTracker;
        }

        /// <summary>
        /// Stores the local flow cache to the global flow table.
        /// </summary>
        /// <param name="flowTracker">A flow tracker object that contains a local flow cache.</param>
        private void PopulateFlowTable(IFlowTracker<FlowData> flowTracker, ProgressRecord progressRecord)
        {
            var flowCache = CacheFactory.GetOrCreateFlowCache(m_ignite, FlowCacheName);
            using (var dataStreamer = m_ignite.GetDataStreamer<FlowKey, FlowData>(flowCache.Name))
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
        public sealed class FlowStreamVisitor : IStreamReceiver<FlowKey, FlowData>
        {
            private MergePacketFlowProcessor updateProcessor;
            public FlowStreamVisitor(MergePacketFlowProcessor updateProcessor)
            {
                this.updateProcessor = updateProcessor;
            }
            public void Receive(ICache<FlowKey, FlowData> cache, ICollection<ICacheEntry<FlowKey, FlowData>> entries)
            {
                foreach (var entry in entries)
                {
                    cache.Invoke(entry.Key, updateProcessor, entry.Value);
                }
            }
        }
    }
}

