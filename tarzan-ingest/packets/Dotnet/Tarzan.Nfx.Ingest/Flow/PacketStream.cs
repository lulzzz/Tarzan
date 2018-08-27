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
    public class PacketStream : FlowRecord, IBinarizable
    {
        public IList<Frame> PacketList { get; private set; }
        public string ServiceName { get; set; }

        protected PacketStream(long firstSeen, long lastSeen, long octets, int packets, List<Frame> list)
        {
            FirstSeen = firstSeen;
            LastSeen = lastSeen;
            Octets = octets;
            Packets = packets;
            PacketList = list;
        }
        public static PacketStream From(Frame frame)
        {
            var unixtime = frame.Timestamp.ToUnixTimeMilliseconds();
            return new PacketStream(unixtime, unixtime, frame.Data.Length, 1, new List<Frame> { frame });
        }                     


        public static PacketStream Update(PacketStream ps, Frame f)
        {
            var ts = f.Timestamp.ToUnixTimeMilliseconds();
            ps.FirstSeen = Math.Min(ps.FirstSeen, ts);
            ps.LastSeen = Math.Max(ps.LastSeen, ts);
            ps.Octets += f.Data.Length;
            ps.Packets++;
            ps.PacketList.Add(f);
            return ps;
        }
        /// <summary>
        /// Merges two existing flow records. It takes uuid from the first record.
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static PacketStream Merge(PacketStream f1, PacketStream f2)
        {
            if (f2 == null) return f1;
            if (f1 == null) return f2;            
            return new PacketStream(
                Math.Min(f1.FirstSeen, f2.FirstSeen),
                Math.Max(f1.LastSeen, f2.LastSeen),
                f1.Octets + f2.Octets,
                f1.Packets + f2.Packets,
                f1.PacketList.Concat(f2.PacketList).ToList());
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

        byte[] GetBytes((Packet Packet, PosixTimeval Timeval) capture)
        {
            var buffer = new byte[capture.Packet.Bytes.Length + sizeof(int) + sizeof(long)];
            BitConverter.GetBytes(capture.Timeval.ToUnixTimeMilliseconds()).CopyTo(buffer,0);
            var linkLayer = GetLinkLayer(capture.Packet);
            BitConverter.GetBytes((int)linkLayer).CopyTo(buffer, sizeof(long));
            capture.Packet.Bytes.CopyTo(buffer, sizeof(long) + sizeof(int));
            return buffer;
            
        }

        (Packet packet, PosixTimeval timeval) FromBytes(byte[] buffer)
        {
            var timeval = PosixTimeval_.FromUnixTimeMilliseconds(BitConverter.ToInt64(buffer, 0));
            var linkLayer = (LinkLayers)BitConverter.ToInt32(buffer, sizeof(long));
            var packetData = new byte[buffer.Length - (sizeof(int) + sizeof(long))];
            Buffer.BlockCopy(buffer, sizeof(int) + sizeof(long), packetData, 0, packetData.Length);
            var packet = Packet.ParsePacket(linkLayer, packetData);
            return (packet, timeval);
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            var stream = new MemoryStream();
            TProtocol tProtocol = new TBinaryProtocol(new TStreamTransport(stream, stream));
            this.Write(tProtocol);
            writer.WriteByteArray(nameof(FlowRecord), stream.ToArray());
            writer.WriteString(nameof(ServiceName), ServiceName);
            foreach(var packetBytes in PacketList.Select((x,index) => (bytes:GetBytes(x),index)))
            {
                writer.WriteByteArray($"Packet_{packetBytes.index.ToString("D8")}", packetBytes.bytes);
            }
        }

        public void ReadBinary(IBinaryReader reader)
        {
            var stream = new MemoryStream(reader.ReadByteArray(nameof(FlowRecord)));
            TProtocol tProtocol = new TBinaryProtocol(new TStreamTransport(stream, stream));
            this.Read(tProtocol);
            reader.ReadString(nameof(ServiceName));
            PacketList = new List<Frame>();
            for(var index = 0; ; index++)
            {
                var packet = reader.ReadByteArray($"Packet_{index.ToString("D8")}");
                if (packet != null)
                    PacketList.Add(FromBytes(packet));
                else break;
            }
        }
    }
}
