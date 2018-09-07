using Apache.Ignite.Core.Binary;
using System.Linq;

namespace Tarzan.Nfx.Ingest.Ignite
{
    class FlowTableSerializer : IBinarySerializer
    {
        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            var packetStream = obj as PacketStream;
            writer.WriteString(nameof(PacketStream.FlowUid), packetStream.FlowUid);
            writer.WriteShort(nameof(PacketStream.Protocol), packetStream.Protocol);
            writer.WriteString(nameof(PacketStream.SourceAddress), packetStream.SourceAddress.ToString());
            writer.WriteInt(nameof(PacketStream.SourcePort), packetStream.SourcePort);
            writer.WriteString(nameof(PacketStream.DestinationAddress), packetStream.DestinationAddress.ToString());
            writer.WriteInt(nameof(PacketStream.DestinationPort), packetStream.DestinationPort);

            writer.WriteLong(nameof(PacketStream.FirstSeen), packetStream.FirstSeen);
            writer.WriteLong(nameof(PacketStream.LastSeen), packetStream.LastSeen);
            writer.WriteLong(nameof(PacketStream.Octets), packetStream.Octets);
            writer.WriteInt(nameof(PacketStream.Packets), packetStream.Packets);
            writer.WriteString(nameof(PacketStream.ServiceName), packetStream.ServiceName);
            writer.WriteArray(nameof(PacketStream.FrameList), packetStream.FrameList.ToArray());
        }

        public void ReadBinary(object obj, IBinaryReader reader)
        {
            var packetStream = obj as PacketStream;
            packetStream.FlowUid = reader.ReadString(nameof(PacketStream.FlowUid));
            packetStream.Protocol = reader.ReadShort(nameof(PacketStream.Protocol));
            packetStream.SourceAddress = reader.ReadString(nameof(PacketStream.SourceAddress));
            packetStream.SourcePort = reader.ReadInt(nameof(PacketStream.SourcePort));
            packetStream.DestinationAddress = reader.ReadString(nameof(PacketStream.DestinationAddress));
            packetStream.DestinationPort = reader.ReadInt(nameof(PacketStream.DestinationPort));
            packetStream.FirstSeen = reader.ReadLong(nameof(PacketStream.FirstSeen));
            packetStream.LastSeen = reader.ReadLong(nameof(PacketStream.LastSeen));
            packetStream.Octets = reader.ReadLong(nameof(PacketStream.Octets));
            packetStream.Packets = reader.ReadInt(nameof(PacketStream.Packets));
            packetStream.ServiceName = reader.ReadString(nameof(PacketStream.ServiceName));
            packetStream.FrameList = reader.ReadArray<Frame>(nameof(PacketStream.FrameList)).ToList();
        }
    }
}
