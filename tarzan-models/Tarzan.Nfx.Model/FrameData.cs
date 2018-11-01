using Apache.Ignite.Core.Binary;

namespace Tarzan.Nfx.Model
{
    public class FrameData : IBinarizable
    {
        public long Timestamp { get; set; }

        public LinkLayerType LinkLayer { get; set; }

        public byte[] Data { get; set; }

        public void ReadBinary(IBinaryReader reader)
        {
            this.Timestamp = reader.ReadLong(nameof(Timestamp));
            this.LinkLayer = (LinkLayerType)reader.ReadInt(nameof(LinkLayer));
            this.Data = reader.ReadByteArray(nameof(Data));
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteLong(nameof(Timestamp), Timestamp);
            writer.WriteInt(nameof(LinkLayer), (int)LinkLayer);
            writer.WriteByteArray(nameof(Data), Data); 
        }
    }
}
