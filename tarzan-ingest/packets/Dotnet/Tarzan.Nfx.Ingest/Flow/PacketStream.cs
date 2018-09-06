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
    /// Represents a single flow, which consists of the flow key, timestampts and counters, and a stream of packets.
    /// This object implements <see cref="IBinarizable"/> and thus can be used as the value type of <see cref="Apache.Ignite.Core.Cache.ICache{TK, TV}"/>.
    /// </summary>
    public partial class PacketStream : IBinarizable
    {
        protected PacketStream(short protocol, ReadOnlySpan<byte> sourceAddress, int sourcePort, ReadOnlySpan<byte> destinationAddress, int destinationPort, long firstSeen, long lastSeen, long octets, int packets, List<Frame> list)
        {
            Protocol = (short)protocol;
            SourceAddress = sourceAddress.ToArray();
            SourcePort = sourcePort;
            DestinationAddress = destinationAddress.ToArray();
            DestinationPort = destinationPort;
            FirstSeen = firstSeen;
            LastSeen = lastSeen;
            Octets = octets;
            Packets = packets;
            FrameList = list;
        }
        /// <summary>
        /// Creates a flow with a single packets.
        /// </summary>
        /// <param name="key">The flow key.</param>
        /// <param name="frame">A frame used to initilzies the flow from.</param>
        /// <returns>A new flow object that contains a single frame.</returns>
        public static PacketStream From(FlowKey key, Frame frame)
        {
            return new PacketStream((short)key.Protocol, key.SourceAddress, key.SourcePort, 
                key.DestinationAddress, key.DestinationPort, frame.Timestamp, frame.Timestamp, frame.Data.Length, 1, new List<Frame> { frame });
        }                     

        /// <summary>
        /// Updates the flow with the provided packet.
        /// </summary>
        /// <param name="packetStream"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
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
        /// Merges two existing flow records. If either input is null the other parameter is returned.
        /// </summary>
        /// <param name="packetStream1"></param>
        /// <param name="packetStream2"></param>
        /// <returns></returns>
        public static PacketStream Merge(PacketStream packetStream1, PacketStream packetStream2)
        {
            if (packetStream2 == null) return packetStream1;
            if (packetStream1 == null) return packetStream2;            
            return new PacketStream(
                packetStream1.Protocol, 
                packetStream1.SourceAddress, 
                packetStream1.SourcePort,
                packetStream1.DestinationAddress,
                packetStream1.DestinationPort,
                Math.Min(packetStream1.FirstSeen, packetStream2.FirstSeen),
                Math.Max(packetStream1.LastSeen, packetStream2.LastSeen),
                packetStream1.Octets + packetStream2.Octets,
                packetStream1.Packets + packetStream2.Packets,
                packetStream1.FrameList.Concat(packetStream2.FrameList).ToList());
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteShort(nameof(Protocol), this.Protocol);
            writer.WriteByteArray(nameof(SourceAddress), this.SourceAddress);
            writer.WriteInt(nameof(SourcePort), this.SourcePort);
            writer.WriteByteArray(nameof(DestinationAddress), this.DestinationAddress);
            writer.WriteInt(nameof(DestinationPort), this.DestinationPort);

            writer.WriteLong(nameof(FirstSeen), this.FirstSeen);
            writer.WriteLong(nameof(LastSeen), this.LastSeen);
            writer.WriteLong(nameof(Octets), this.Octets);
            writer.WriteInt(nameof(Packets), this.Packets);
            writer.WriteString(nameof(ServiceName), this.ServiceName);
            writer.WriteArray(nameof(FrameList), this.FrameList.ToArray());
        }

        public void ReadBinary(IBinaryReader reader)
        {
            this.Protocol = reader.ReadShort(nameof(Protocol));
            this.SourceAddress = reader.ReadByteArray(nameof(SourceAddress));
            this.SourcePort = reader.ReadInt(nameof(SourcePort));
            this.DestinationAddress = reader.ReadByteArray(nameof(DestinationAddress));
            this.DestinationPort = reader.ReadInt(nameof(DestinationPort));
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
