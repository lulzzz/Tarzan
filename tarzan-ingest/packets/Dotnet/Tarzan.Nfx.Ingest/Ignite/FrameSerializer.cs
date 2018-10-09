using Apache.Ignite.Core.Binary;
using Netdx.PacketDecoders;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.FlowTracker
{
    /// <summary>
    /// Represents a raw captured link layer frame.
    /// </summary>
    public partial class FrameSerializer : IBinarySerializer
    {                                     

        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            var frame = (Frame)obj;

            writer.WriteInt(nameof(Frame.LinkLayer), (int)frame.LinkLayer);
            writer.WriteLong(nameof(Frame.Timestamp), frame.Timestamp);
            writer.WriteByteArray(nameof(Frame.Data), frame.Data);
        }

        public void ReadBinary(object obj, IBinaryReader reader)
        {
            var frame = (Frame)obj;

            frame.LinkLayer = (LinkLayerType)reader.ReadInt(nameof(Frame.LinkLayer));
            frame.Timestamp = reader.ReadLong(nameof(Frame.Timestamp));
            frame.Data = reader.ReadByteArray(nameof(Frame.Data));
        }
    }
}
