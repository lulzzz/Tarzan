using Netdx.Packets.Base;
using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace Netdx.PacketDecoders
{
    public struct PacketFlowKey
    {
        static class Fields
        {
            internal const int ProtocolPosition = 0;
            internal const int ProtocolFamilyPosition = 1;
            internal const int SourceAddressPosition = 4;
            internal const int SourcePortPosition = 20;
            internal const int DestinationAddressPosition = 22;
            internal const int DestinationPortPosition = 38;
        }
        /// <summary>
        /// Flow key contains 40 bytes (aligned):
        /// |  0 |  1 | protocol
        /// |  1 |  2 | address family
        /// |  2 |  2 | reserved
        /// |  4 | 16 | source ip
        /// | 20 |  2 | source port
        /// | 22 | 16 | destin ip
        /// | 38 |  2 | destin port
        /// </summary>
        private readonly byte[] _bytes;
        private readonly int _hashCode;

        public int HashCode => _hashCode;

        public static PacketFlowKey Create(byte protocol, Span<byte> sourceAddres, UInt16 sourcePort, Span<byte> destinationAddress, UInt16 destinationPort)
        {
            var bytes = new byte[40];
            bytes[Fields.ProtocolPosition] = protocol;
            bytes[Fields.ProtocolFamilyPosition] = (byte)(sourceAddres.Length == 4 ? ProtocolFamily.InterNetwork : ProtocolFamily.InterNetworkV6);
            sourceAddres.CopyTo(new Span<byte>(bytes, Fields.SourceAddressPosition, 16));
            destinationAddress.CopyTo(new Span<byte>(bytes, Fields.DestinationAddressPosition, 16));
            BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(bytes, Fields.SourcePortPosition,2), sourcePort);
            BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(bytes, Fields.DestinationPortPosition, 2), destinationPort);
            return new PacketFlowKey(bytes);
        }

        public override bool Equals(object obj)
        {
            var that = (PacketFlowKey)obj;
            return Compare(this, that);
        }

        public override int GetHashCode()
        {
            return this.HashCode;
        }
        public override string ToString()
        {
            return $"{(ProtocolType)Protocol}:{SourceIpAddress}:{SourcePort}>{DestinationIpAddress}:{DestinationPort}";
        }

        IPAddress SourceIpAddress => new IPAddress(SourceAddress.ToArray());
        IPAddress DestinationIpAddress => new IPAddress(DestinationAddress.ToArray());
        public PacketFlowKey(byte[] bytes)
        {

            if (bytes.Length != 40) throw new ArgumentException();
            this._bytes = bytes;
            this._hashCode = GetHashCode(bytes);
        }
        public static bool Compare(PacketFlowKey f1, PacketFlowKey f2)
        {
            return f1._hashCode == f2._hashCode && new Span<byte>(f1._bytes).SequenceEqual(f2._bytes);
        }
        public static unsafe int GetHashCode(Span<byte> bytes)
        {
            fixed (byte* bytePtr = bytes)
            {
                var intPtr = (int*)bytePtr;

                return intPtr[0] ^ intPtr[1] ^ intPtr[2] ^ intPtr[3] ^ intPtr[4]
                    ^ intPtr[5] ^ intPtr[6] ^ intPtr[7] ^ intPtr[8] ^ intPtr[9];
            }
        }
        public ProtocolType Protocol => (ProtocolType)_bytes[Fields.ProtocolPosition];
        Span<Byte> ProtocolByte => new Span<byte>(_bytes,Fields.ProtocolPosition, 1);

        public ProtocolFamily ProtocolFamily => (ProtocolFamily)_bytes[Fields.ProtocolFamilyPosition];
        Span<Byte> ProtocolFamilyByte => new Span<byte>(_bytes,Fields.ProtocolFamilyPosition, 1);

        public ReadOnlySpan<byte> SourceAddress => new Span<byte>(_bytes, Fields.SourceAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
        Span<Byte> SourceAddressBytes => new Span<byte>(_bytes, Fields.SourceAddressPosition, 16);

        public ReadOnlySpan<byte> DestinationAddress => new Span<byte>(_bytes, Fields.DestinationAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
        Span<Byte> DestinationAddressBytes => new Span<byte>(_bytes, Fields.DestinationAddressPosition, 16);

        public UInt16 SourcePort => BinaryPrimitives.ReadUInt16BigEndian(SourcePortBytes);
        Span<Byte> SourcePortBytes => new Span<byte>(_bytes, Fields.SourcePortPosition ,2);

        public UInt16 DestinationPort => BinaryPrimitives.ReadUInt16BigEndian(DestinationPortBytes);
        Span<Byte> DestinationPortBytes => new Span<byte>(_bytes, Fields.DestinationPortPosition, 2);


        public static PacketFlowKey GetKey(byte[] bytes)
        {
            var etherType = EthernetFrame.GetEtherType(bytes);
            var etherPayload = EthernetFrame.GetPayloadBytes(bytes);

            Span<Byte> ipPayload = stackalloc byte[0];
            var protocol = 0;
            Span<Byte> sourceAddress = stackalloc byte[4];
            Span<Byte> destinAddress = stackalloc byte[4];
            UInt16 sourcePort = 0;
            UInt16 destinPort = 0;
            switch (etherType)
            {
                case (ushort)EthernetFrame.EtherTypeEnum.Ipv4:
                    {
                        sourceAddress = Ipv4Packet.GetSourceAddress(etherPayload);
                        destinAddress = Ipv4Packet.GetDestinationAddress(etherPayload);
                        ipPayload = Ipv4Packet.GetPayloadBytes(etherPayload);
                        protocol = Ipv4Packet.GetProtocol(etherPayload);
                        break;
                    }
                case (ushort)EthernetFrame.EtherTypeEnum.Ipv6:
                    {
                        sourceAddress = Ipv6Packet.GetSourceAddress(etherPayload);
                        destinAddress = Ipv6Packet.GetDestinationAddress(etherPayload);
                        ipPayload = Ipv6Packet.GetPayloadBytes(etherPayload);
                        protocol = Ipv4Packet.GetProtocol(etherPayload);
                        break;
                    }
                default:
                    break;
            }
            switch (protocol)
            {
                case 6:  // TCP
                    {
                        sourcePort = TcpSegment.GetSourcePort(ipPayload);
                        destinPort = TcpSegment.GetDestinationPort(ipPayload);
                        break;
                    }
                case 17: // UDP
                    {
                        sourcePort = UdpDatagram.GetSourcePort(ipPayload);
                        destinPort = UdpDatagram.GetDestinationPort(ipPayload);
                        break;
                    }
                default:
                    break;
            }
            // ok we have enough information for creating packet's flow key
            var flowKey = PacketFlowKey.Create((byte)protocol, sourceAddress, sourcePort, destinAddress, destinPort);
            return flowKey;
        }

    }
}
