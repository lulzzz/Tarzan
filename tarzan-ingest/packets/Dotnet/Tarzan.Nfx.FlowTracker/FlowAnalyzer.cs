using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.FlowTracker
{
    class FlowAnalyzer : IComputeAction
    {
        public string CacheName { get; set; }

        [InstanceResource]
        private readonly IIgnite m_ignite;

        public void Invoke()
        {
            var flowTracker = TrackFlows();
            PopulateFlowTable(flowTracker);
        }

        private IFlowTracker<FlowRecord> TrackFlows()
        {
            var cache = m_ignite.GetCache<int, Frame>(CacheName);

            var flowTracker = new FlowTracker(new FrameKeyProvider());
            foreach (var frame in cache.GetLocalEntries())
            {
                flowTracker.ProcessFrame(frame.Value);
            }
            return flowTracker;
        }

        /// <summary>
        /// Loads a local flow cache to the global flow table.
        /// </summary>
        /// <param name="flowTracker">A flow tracker object that contains a local flow cache.</param>
        private void PopulateFlowTable(IFlowTracker<FlowRecord> flowTracker)
        {
            var flowCache = m_ignite.GetOrCreateCache<FlowKey, PacketFlow>("flowtable");
            var updateProcessor = new MergePacketFlowProcessor();

            foreach (var flow in flowTracker.FlowTable)
            {
                flow.Value.Flow.FlowUid = FlowUidGenerator.NewUid(flow.Key, flow.Value.Flow.FirstSeen);
                flowCache.Invoke(flow.Key, updateProcessor, flow.Value.Flow);
            }
        }
    }
}

