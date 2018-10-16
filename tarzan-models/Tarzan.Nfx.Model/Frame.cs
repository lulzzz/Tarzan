using Apache.Ignite.Core.Binary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    public partial class Frame : IBinarizable
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
