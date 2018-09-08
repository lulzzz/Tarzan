using Apache.Ignite.Core.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Ingest.Utils;

namespace Tarzan.Nfx.Ingest
{
    /// <summary>
    /// Represents a single flow, which consists of the flow key, timestampts and counters, and a stream of packets.
    /// </summary>
    public partial class PacketStream
    {
        protected PacketStream(short protocol, ReadOnlySpan<byte> sourceAddress, int sourcePort, ReadOnlySpan<byte> destinationAddress, int destinationPort, long firstSeen, long lastSeen, long octets, int packets, List<Frame> list)
        {
            Protocol = (short)protocol;
            SourceAddressBytes = sourceAddress.ToArray();
            SourcePort = sourcePort;
            DestinationAddressBytes = destinationAddress.ToArray();
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
        
        public InternetAddress SourceAddress
        {
            get
            {
                return new InternetAddress(SourceAddressBytes);
            }
            set
            {
                SourceAddressBytes = value.GetAddressBytes();
            }
        }
        public InternetAddress DestinationAddress
        {
            get
            {
                return new InternetAddress(DestinationAddressBytes);
            }
            set
            {
                DestinationAddressBytes = value.GetAddressBytes();
            }
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
            if (packetStream1 == null) throw new ArgumentNullException(nameof(packetStream1));
            if (packetStream2 == null) throw new ArgumentNullException(nameof(packetStream2));

            return new PacketStream(
                packetStream1.Protocol,
                packetStream1.SourceAddressBytes,
                packetStream1.SourcePort,
                packetStream1.DestinationAddressBytes,
                packetStream1.DestinationPort,
                Math.Min(packetStream1.FirstSeen, packetStream2.FirstSeen),
                Math.Max(packetStream1.LastSeen, packetStream2.LastSeen),
                packetStream1.Octets + packetStream2.Octets,
                packetStream1.Packets + packetStream2.Packets,
                packetStream1.FrameList.Concat(packetStream2.FrameList).ToList())
            {
                FlowUid = packetStream1.FirstSeen < packetStream2.FirstSeen ? packetStream1.FlowUid : packetStream2.FlowUid
            };
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
