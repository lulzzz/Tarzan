using Apache.Ignite.Core.Cache;
using System;
using Tarzan.Nfx.Ingest.Flow;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class MergePacketFlowProcessor : ICacheEntryProcessor<FlowKey, PacketFlow, PacketFlow, PacketFlow>
    {
        public PacketFlow Process(IMutableCacheEntry<FlowKey, PacketFlow> entry, PacketFlow arg)
        {
            if (entry.Exists)
            {
                var flowUid = FlowUidGenerator.NewUid(entry.Key, Math.Min(entry.Value.FirstSeen, arg.FirstSeen));
                entry.Value = PacketFlowFactory.Merge(entry.Value, arg, flowUid.ToString());
            }
            else
            {
                entry.Value = arg;
            }
            return null;
        }
    }

}
