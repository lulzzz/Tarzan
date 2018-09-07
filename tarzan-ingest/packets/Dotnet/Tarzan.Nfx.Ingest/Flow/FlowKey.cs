using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using Netdx.Packets.Base;
using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace Tarzan.Nfx.Ingest
{
    /// <summary>
    /// A compact representation of a flow key.
    /// </summary>
    [Serializable]
    public class FlowKey : IBinarizable
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
        private byte[] m_bytes;
        private int m_hashCode;

        public override bool Equals(object obj)
        {
            var that = (FlowKey)obj;
            return Compare(this, that);
        }

        public override int GetHashCode()
        {
            return this.m_hashCode;
        }
        public override string ToString()
        {
            return $"{(ProtocolType)Protocol}:{SourceIpAddress}:{SourcePort}>{DestinationIpAddress}:{DestinationPort}";
        }

        public IPAddress SourceIpAddress => new IPAddress(SourceAddress.ToArray());

        public IPAddress DestinationIpAddress => new IPAddress(DestinationAddress.ToArray());

        public FlowKey(byte[] bytes)
        {
            if (bytes.Length != 40) throw new ArgumentException("Invalid size of input array. Must be exactly 40 bytes.");
            this.m_bytes = bytes;
            this.m_hashCode = GetHashCode(bytes);
        }

        public ProtocolType Protocol => (ProtocolType)m_bytes[Fields.ProtocolPosition];
        internal Span<Byte> ProtocolByte => new Span<byte>(m_bytes,Fields.ProtocolPosition, 1);


        public ProtocolFamily ProtocolFamily => (ProtocolFamily)m_bytes[Fields.ProtocolFamilyPosition];
         Span<Byte> ProtocolFamilyByte => new Span<byte>(m_bytes,Fields.ProtocolFamilyPosition, 1);

        public ReadOnlySpan<byte> SourceAddress => new Span<byte>(m_bytes, Fields.SourceAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
         Span<byte> SourceAddressBytes => new Span<byte>(m_bytes, Fields.SourceAddressPosition, 16);

        public ReadOnlySpan<byte> DestinationAddress => new Span<byte>(m_bytes, Fields.DestinationAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
         Span<Byte> DestinationAddressBytes => new Span<byte>(m_bytes, Fields.DestinationAddressPosition, 16);

        public UInt16 SourcePort => BinaryPrimitives.ReadUInt16BigEndian(SourcePortBytes);
         Span<Byte> SourcePortBytes => new Span<byte>(m_bytes, Fields.SourcePortPosition ,2);

        public UInt16 DestinationPort => BinaryPrimitives.ReadUInt16BigEndian(DestinationPortBytes);
         Span<Byte> DestinationPortBytes => new Span<byte>(m_bytes, Fields.DestinationPortPosition, 2);

        public IPEndPoint SourceEndpoint => new IPEndPoint(new IPAddress(SourceAddress.ToArray()), SourcePort);
        public IPEndPoint DestinationEndpoint => new IPEndPoint(new IPAddress(DestinationAddress.ToArray()), DestinationPort);

        public byte[] Bytes => m_bytes;

        public void WriteBinary(IBinaryWriter writer)
        { 
            writer.WriteByteArray(nameof(this.m_bytes), this.m_bytes);
            writer.WriteInt(nameof(this.m_hashCode), this.m_hashCode);
        }

        public void ReadBinary(IBinaryReader reader)
        {
            m_bytes = reader.ReadByteArray(nameof(m_bytes));
            if (m_bytes == null || m_bytes.Length != 40)
            {
                throw new ArgumentOutOfRangeException($"Invalid size of {nameof(FlowKey.m_bytes)}. Must be exactly 40 bytes.");
            }

            m_hashCode = reader.ReadInt(nameof(m_hashCode));
        }

        public static FlowKey Create(byte protocol, Span<byte> sourceAddres, ushort sourcePort, Span<byte> destinationAddress, ushort destinationPort)
        {
            var bytes = new byte[40];
            bytes[Fields.ProtocolPosition] = protocol;
            bytes[Fields.ProtocolFamilyPosition] = (byte)(sourceAddres.Length == 4 ? ProtocolFamily.InterNetwork : ProtocolFamily.InterNetworkV6);
            sourceAddres.CopyTo(new Span<byte>(bytes, Fields.SourceAddressPosition, 16));
            destinationAddress.CopyTo(new Span<byte>(bytes, Fields.DestinationAddressPosition, 16));
            BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(bytes, Fields.SourcePortPosition, 2), sourcePort);
            BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(bytes, Fields.DestinationPortPosition, 2), destinationPort);
            return new FlowKey(bytes);
        }
        public static bool Compare(FlowKey f1, FlowKey f2)
        {
            return f1.m_hashCode == f2.m_hashCode && new Span<byte>(f1.m_bytes).SequenceEqual(f2.m_bytes);
        }
        static unsafe int GetHashCode(Span<byte> bytes)
        {
            fixed (byte* bytePtr = bytes)
            {
                var intPtr = (int*)bytePtr;
                return intPtr[0] ^ intPtr[1] ^ intPtr[2] ^ intPtr[3] ^ intPtr[4]
                    ^ intPtr[5] ^ intPtr[6] ^ intPtr[7] ^ intPtr[8] ^ intPtr[9];
            }
        }
    }
}
