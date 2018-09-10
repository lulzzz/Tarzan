using Apache.Ignite.Core.Binary;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    class PacketFlowTableSerializer : IBinarySerializer
    {
        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            var packetFlow = obj as PacketFlow;
            writer.WriteString(nameof(PacketFlow.FlowUid), packetFlow.FlowUid);
            writer.WriteString(nameof(PacketFlow.Protocol), packetFlow.Protocol);
            writer.WriteString(nameof(PacketFlow.SourceAddress), packetFlow.SourceAddress.ToString());
            writer.WriteInt(nameof(PacketFlow.SourcePort), packetFlow.SourcePort);
            writer.WriteString(nameof(PacketFlow.DestinationAddress), packetFlow.DestinationAddress.ToString());
            writer.WriteInt(nameof(PacketFlow.DestinationPort), packetFlow.DestinationPort);

            writer.WriteLong(nameof(PacketFlow.FirstSeen), packetFlow.FirstSeen);
            writer.WriteLong(nameof(PacketFlow.LastSeen), packetFlow.LastSeen);
            writer.WriteLong(nameof(PacketFlow.Octets), packetFlow.Octets);
            writer.WriteInt(nameof(PacketFlow.Packets), packetFlow.Packets);
            writer.WriteString(nameof(PacketFlow.ServiceName), packetFlow.ServiceName);
        }

        public void ReadBinary(object obj, IBinaryReader reader)
        {
            var packetFlow = obj as PacketFlow;
            packetFlow.FlowUid = reader.ReadString(nameof(PacketFlow.FlowUid));
            packetFlow.Protocol = reader.ReadString(nameof(PacketFlow.Protocol));
            packetFlow.SourceAddress = reader.ReadString(nameof(PacketFlow.SourceAddress));
            packetFlow.SourcePort = reader.ReadInt(nameof(PacketFlow.SourcePort));
            packetFlow.DestinationAddress = reader.ReadString(nameof(PacketFlow.DestinationAddress));
            packetFlow.DestinationPort = reader.ReadInt(nameof(PacketFlow.DestinationPort));
            packetFlow.FirstSeen = reader.ReadLong(nameof(PacketFlow.FirstSeen));
            packetFlow.LastSeen = reader.ReadLong(nameof(PacketFlow.LastSeen));
            packetFlow.Octets = reader.ReadLong(nameof(PacketFlow.Octets));
            packetFlow.Packets = reader.ReadInt(nameof(PacketFlow.Packets));
            packetFlow.ServiceName = reader.ReadString(nameof(PacketFlow.ServiceName));
        }
    }
}
