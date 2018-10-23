using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers
{
    public partial class FlowAnalyzer
    {
        public class MergePacketFlowProcessor : ICacheEntryProcessor<FlowKey, PacketFlow, PacketFlow, PacketFlow>
        {
            public PacketFlow Process(IMutableCacheEntry<FlowKey, PacketFlow> entry, PacketFlow arg)
            {
                if (entry.Exists)
                {
                    var flowUid = FlowUidGenerator.NewUid(entry.Key, Math.Min(entry.Value.FirstSeen, arg.FirstSeen));
                    entry.Value = Merge(entry.Value, arg, flowUid.ToString());
                }
                else
                {
                    entry.Value = arg;
                }

                return null;
            }

            public static PacketFlow Merge(PacketFlow flow1, PacketFlow flow2, string flowUid)
            {
                if (flow1 == null) throw new ArgumentNullException(nameof(flow1));
                if (flow2 == null) throw new ArgumentNullException(nameof(flow2));

                return new PacketFlow()
                {
                    FlowUid = flowUid,
                    Protocol = flow1.Protocol,
                    SourceAddress = flow1.SourceAddress,
                    SourcePort = flow1.SourcePort,
                    DestinationAddress = flow1.DestinationAddress,
                    DestinationPort = flow1.DestinationPort,
                    FirstSeen = Math.Min(flow1.FirstSeen, flow2.FirstSeen),
                    LastSeen = Math.Max(flow1.LastSeen, flow2.LastSeen),
                    Octets = flow1.Octets + flow2.Octets,
                    Packets = flow1.Packets + flow2.Packets
                };
            }
        }
    }
}
