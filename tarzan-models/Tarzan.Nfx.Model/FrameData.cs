using Apache.Ignite.Core.Binary;
using System;

namespace Tarzan.Nfx.Model
{
    [Serializable]
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

        public override bool Equals(object obj)
        {
            if (!(obj is FrameData other)) return false;
            return this.LinkLayer == other.LinkLayer
                && this.Timestamp == other.Timestamp
                && new Span<byte>(this.Data).SequenceEqual(other.Data);
        }
    }
}
