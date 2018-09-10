using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using Tarzan.Nfx.Ingest.Flow;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public static class PacketFlowExtension
    {
        public static bool IntersectsWith(this PacketFlow @this, PacketFlow that)
        {
            if (@this.FirstSeen <= that.FirstSeen)
            {
                return that.FirstSeen <= @this.LastSeen;
            }
            else
            {
                return @this.FirstSeen <= that.LastSeen;
            }
        }
    }

    public class PacketFlowFactory : ITableConfigurationProvider
    {
        public static PacketFlow From(FlowKey flowKey, Frame frame, string flowUid)
        {
            return new PacketFlow()
            {
                FlowUid = flowUid,
                Protocol = flowKey.Protocol.ToString(),
                SourceAddress = flowKey.SourceIpAddress.ToString(),
                SourcePort = flowKey.SourcePort,
                DestinationAddress = flowKey.DestinationIpAddress.ToString(),
                DestinationPort = flowKey.DestinationPort,
                FirstSeen = frame.Timestamp,
                LastSeen = frame.Timestamp,
                Octets = frame.Data.Length,
                Packets = 1
            };
        }

        public static PacketFlow Update(PacketFlow packetFlow, Frame frame)
        {
            packetFlow.FirstSeen = Math.Min(packetFlow.FirstSeen, frame.Timestamp);
            packetFlow.LastSeen = Math.Max(packetFlow.LastSeen, frame.Timestamp);
            packetFlow.Octets += frame.Data.Length;
            packetFlow.Packets++;
            return packetFlow;
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

        public CacheConfiguration CacheConfiguration =>
            new CacheConfiguration(nameof(PacketFlowTable))
            {
                CacheMode = CacheMode.Partitioned,
                Backups = 0,
                QueryEntities = new[]
                {
                    new QueryEntity(typeof(PacketStream))
                    {
                        Fields = new []
                        {
                            new QueryField(nameof(PacketFlow.FlowUid), typeof(string)),
                            new QueryField(nameof(PacketFlow.Protocol), typeof(short)),
                            new QueryField(nameof(PacketFlow.SourceAddress), typeof(string)),
                            new QueryField(nameof(PacketFlow.SourcePort), typeof(int)),
                            new QueryField(nameof(PacketFlow.DestinationAddress), typeof(string)),
                            new QueryField(nameof(PacketFlow.DestinationPort), typeof(int)),
                            new QueryField(nameof(PacketFlow.ServiceName), typeof(string)),
                            new QueryField(nameof(PacketFlow.FirstSeen), typeof(long)),
                            new QueryField(nameof(PacketFlow.LastSeen), typeof(long)),
                            new QueryField(nameof(PacketFlow.Packets), typeof(int)),
                            new QueryField(nameof(PacketFlow.Octets), typeof(long)),
                        }
                    }
                }
            };

        public BinaryTypeConfiguration TypeConfiguration => new BinaryTypeConfiguration()
        {
            TypeName = nameof(PacketFlow),
            Serializer = new PacketFlowTableSerializer()
        };
    }
}
