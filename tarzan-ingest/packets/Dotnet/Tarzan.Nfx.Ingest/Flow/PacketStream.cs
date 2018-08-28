using Apache.Ignite.Core.Binary;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thrift.Protocol;
using Thrift.Transport;

namespace Tarzan.Nfx.Ingest
{
    /// <summary>
    /// Extends <see cref="FlowRecord"/> with other properties, such as, <see cref="FlowId"/>, <see cref="ServiceName"/>, and mainly
    /// <see cref="PacketList"/>.
    /// </summary>
    public partial class PacketStream : IBinarizable
    {

        protected PacketStream(long firstSeen, long lastSeen, long octets, int packets, List<Frame> list)
        {
            FirstSeen = firstSeen;
            LastSeen = lastSeen;
            Octets = octets;
            Packets = packets;
            FrameList = list;
        }
        public static PacketStream From(Frame frame)
        {
            return new PacketStream(frame.Timestamp, frame.Timestamp, frame.Data.Length, 1, new List<Frame> { frame });
        }                     


        public static PacketStream Update(PacketStream packetStream, Frame frame)
        {
            packetStream.FirstSeen = Math.Min(packetStream.FirstSeen, frame.Timestamp);
            packetStream.LastSeen = Math.Max(packetStream.LastSeen, frame.Timestamp);
            packetStream.Octets += frame.Data.Length;
            packetStream.Packets++;
            packetStream.FrameList.Add(frame);
            return packetStream;
        }
        /// <summary>
        /// Merges two existing flow records. It takes uuid from the first record.
        /// </summary>
        /// <param name="packetStream1"></param>
        /// <param name="packetStream2"></param>
        /// <returns></returns>
        public static PacketStream Merge(PacketStream packetStream1, PacketStream packetStream2)
        {
            if (packetStream2 == null) return packetStream1;
            if (packetStream1 == null) return packetStream2;            
            return new PacketStream(
                Math.Min(packetStream1.FirstSeen, packetStream2.FirstSeen),
                Math.Max(packetStream1.LastSeen, packetStream2.LastSeen),
                packetStream1.Octets + packetStream2.Octets,
                packetStream1.Packets + packetStream2.Packets,
                packetStream1.FrameList.Concat(packetStream2.FrameList).ToList());
        }

        LinkLayers GetLinkLayer(Packet packet)
        {
            switch(packet)
            {
                case EthernetPacket _: return LinkLayers.Ethernet;
                case RawIPPacket _: return LinkLayers.Raw;
                case PPPPacket _: return LinkLayers.Ppp;
                case LinuxSLLPacket _: return LinkLayers.LinuxSLL;
                case RadioPacket _: return LinkLayers.Ieee80211_Radio;
                default: return LinkLayers.Null;
            }
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            var stream = new MemoryStream();
            TProtocol tProtocol = new TBinaryProtocol(new TStreamTransport(stream, stream));
            this.Write(tProtocol);
            writer.GetRawWriter().WriteByteArray(stream.ToArray());
        }

        public void ReadBinary(IBinaryReader reader)
        {
            var stream = new MemoryStream(reader.GetRawReader().ReadByteArray());
            TProtocol tProtocol = new TBinaryProtocol(new TStreamTransport(stream, stream));
            this.Read(tProtocol);
        }
    }
}
