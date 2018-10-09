using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace Tarzan.Nfx.FlowTracker
{
    /// <summary>
    /// A compact representation of a flow key.
    /// </summary>
    [Serializable]
    public class FlowKey 
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

        public byte[] Bytes { get; private set; }

        public int HashCode { get; private set; }

        public override bool Equals(object obj)
        {
            var that = (FlowKey)obj;
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

        public IPAddress SourceIpAddress => new IPAddress(SourceAddress.ToArray());

        public IPAddress DestinationIpAddress => new IPAddress(DestinationAddress.ToArray());

        public FlowKey(byte[] bytes)
        {
            if (bytes.Length != 40) throw new ArgumentException("Invalid size of input array. Must be exactly 40 bytes.");
            this.Bytes = bytes;
            this.HashCode = GetHashCode(bytes);
        }

        public void Reload(byte[] bytes)
        {
            if (bytes.Length != 40) throw new ArgumentException("Invalid size of input array. Must be exactly 40 bytes.");
            this.Bytes = bytes;
            this.HashCode = GetHashCode(bytes);
        }

        public ProtocolType Protocol => (ProtocolType)Bytes[Fields.ProtocolPosition];
        internal Span<Byte> ProtocolByte => new Span<byte>(Bytes,Fields.ProtocolPosition, 1);


        public ProtocolFamily ProtocolFamily => (ProtocolFamily)Bytes[Fields.ProtocolFamilyPosition];
         Span<Byte> ProtocolFamilyByte => new Span<byte>(Bytes,Fields.ProtocolFamilyPosition, 1);

        public ReadOnlySpan<byte> SourceAddress => new Span<byte>(Bytes, Fields.SourceAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
         Span<byte> SourceAddressBytes => new Span<byte>(Bytes, Fields.SourceAddressPosition, 16);

        public ReadOnlySpan<byte> DestinationAddress => new Span<byte>(Bytes, Fields.DestinationAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
         Span<Byte> DestinationAddressBytes => new Span<byte>(Bytes, Fields.DestinationAddressPosition, 16);

        public UInt16 SourcePort => BinaryPrimitives.ReadUInt16BigEndian(SourcePortBytes);
         Span<Byte> SourcePortBytes => new Span<byte>(Bytes, Fields.SourcePortPosition ,2);

        public UInt16 DestinationPort => BinaryPrimitives.ReadUInt16BigEndian(DestinationPortBytes);
         Span<Byte> DestinationPortBytes => new Span<byte>(Bytes, Fields.DestinationPortPosition, 2);

        public IPEndPoint SourceEndpoint => new IPEndPoint(new IPAddress(SourceAddress.ToArray()), SourcePort);
        public IPEndPoint DestinationEndpoint => new IPEndPoint(new IPAddress(DestinationAddress.ToArray()), DestinationPort);

        public static FlowKey Create(byte protocol, ReadOnlySpan<byte> sourceAddres, ushort sourcePort, ReadOnlySpan<byte> destinationAddress, ushort destinationPort)
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
            return f1.HashCode == f2.HashCode && new Span<byte>(f1.Bytes).SequenceEqual(f2.Bytes);
        }

        private static unsafe int GetHashCode(Span<byte> bytes)
        {
            fixed (byte* bytePtr = bytes)
            {
                var intPtr = (int*)bytePtr;
                return intPtr[0] ^ intPtr[1] ^ intPtr[2] ^ intPtr[3] ^ intPtr[4]
                    ^ intPtr[5] ^ intPtr[6] ^ intPtr[7] ^ intPtr[8] ^ intPtr[9];
            }
        }


        public FlowKey SwapEndpoints()
        {
            return FlowKey.Create((byte)this.Protocol, this.DestinationAddress, this.DestinationPort, this.SourceAddress, this.SourcePort);
        }
    }
}
