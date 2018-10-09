using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Ingest.Flow;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class PacketFlowTable : ICacheFactory<FlowKey, PacketFlow>
    {
       
        private IIgnite m_ignite;

        public PacketFlowTable(IIgnite m_ignite)
        {
            this.m_ignite = m_ignite;
        }

        public ICache<FlowKey, PacketFlow> GetCache()
        {
            return this.m_ignite.GetCache<FlowKey, PacketFlow>(nameof(PacketFlowTable));
        }

        public ICache<FlowKey, PacketFlow> GetOrCreateCache()
        {
            return this.m_ignite.GetOrCreateCache<FlowKey, PacketFlow>(nameof(PacketFlowTable));
        }
    }
}
