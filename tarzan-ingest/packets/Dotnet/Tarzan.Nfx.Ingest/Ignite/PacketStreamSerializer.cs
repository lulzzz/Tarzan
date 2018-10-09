using Apache.Ignite.Core.Binary;
using System.Linq;
using Tarzan.Nfx.FlowTracker;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class PacketStreamSerializer : IBinarySerializer
    {
        public void ReadBinary(object obj, IBinaryReader reader)
        {
            var packetStream = (PacketStream)obj;
            packetStream.FlowUid = reader.ReadString(nameof(PacketStream.FlowUid));
            packetStream.FrameList = reader.ReadArray<Frame>(nameof(PacketStream.FrameList))?.ToList();
        }

        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            var packetStream = (PacketStream)obj;
            writer.WriteString(nameof(PacketStream.FlowUid), packetStream.FlowUid);
            writer.WriteArray(nameof(PacketStream.FrameList), packetStream.FrameList?.ToArray());
        }
    }
}
