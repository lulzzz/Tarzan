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
    /// Represents a flow record.
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
            writer.WriteLong(nameof(FirstSeen), this.FirstSeen);
            writer.WriteLong(nameof(LastSeen), this.LastSeen);
            writer.WriteLong(nameof(Octets), this.Octets);
            writer.WriteInt(nameof(Packets), this.Packets);
            writer.WriteString(nameof(ServiceName), this.ServiceName);
            writer.WriteArray<Frame>(nameof(FrameList), this.FrameList.ToArray());
        }

        public void ReadBinary(IBinaryReader reader)
        {
            this.FirstSeen = reader.ReadLong(nameof(FirstSeen));
            this.LastSeen = reader.ReadLong(nameof(LastSeen));
            this.Octets = reader.ReadLong(nameof(Octets));
            this.Packets = reader.ReadInt(nameof(Packets));
            this.ServiceName = reader.ReadString(nameof(ServiceName));
            this.FrameList = reader.ReadArray<Frame>(nameof(FrameList)).ToList();
        }

        public bool IntersectsWith(PacketStream that)
        {
            if (this.FirstSeen <= that.FirstSeen)
            {
                return that.FirstSeen <= this.LastSeen;
            }
            else
            {
                return this.FirstSeen <= that.LastSeen;
            }
        }
    }
}
