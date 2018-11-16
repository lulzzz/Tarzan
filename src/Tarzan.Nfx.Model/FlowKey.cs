using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Affinity;
using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace Tarzan.Nfx.Model
{
    /// <summary>
    /// A compact representation of a flow key.
    /// </summary>
    [Serializable]
    public class FlowKey : IBinarizable
    {
        public byte[] Bytes { get; private set; }

        [AffinityKeyMapped]
        public int FlowKeyHash { get; private set; }

        public FlowKey(byte[] bytes)
        {
            if (bytes.Length != 40) throw new ArgumentException("Invalid size of input array. Must be exactly 40 bytes.");
            this.Bytes = bytes;
            this.FlowKeyHash = GetHashCode(bytes);
        }

        public void Reload(byte[] bytes)
        {
            if (bytes.Length != 40) throw new ArgumentException("Invalid size of input array. Must be exactly 40 bytes.");
            this.Bytes = bytes;
            this.FlowKeyHash = GetHashCode(bytes);
        }

        public override int GetHashCode()
        {
            return this.FlowKeyHash;
        }

        public override string ToString()
        {
            return $"{(ProtocolType)Protocol}:{SourceIpAddress}:{SourcePort}>{DestinationIpAddress}:{DestinationPort}";
        }

        public override bool Equals(object obj)
        {
            var that = (FlowKey)obj;
            return Compare(this, that);
        }

        public FlowKey SwapEndpoints()
        {
            return Create((byte)this.Protocol, this.DestinationAddress, this.DestinationPort, this.SourceAddress, this.SourcePort);
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteByteArray(nameof(FlowKey.Bytes), this.Bytes);
            writer.WriteInt(nameof(FlowKey.FlowKeyHash), this.FlowKeyHash);
        }

        public void ReadBinary(IBinaryReader reader)
        {
            this.Bytes = reader.ReadByteArray(nameof(FlowKey.Bytes));
            this.FlowKeyHash = reader.ReadInt(nameof(FlowKey.FlowKeyHash));
        }

        public IPAddress SourceIpAddress => new IPAddress(SourceAddress.ToArray());
        public IPAddress DestinationIpAddress => new IPAddress(DestinationAddress.ToArray());

        public ProtocolType Protocol => (ProtocolType)Bytes[Fields.ProtocolPosition];
        internal Span<byte> ProtocolByte => new Span<byte>(Bytes, Fields.ProtocolPosition, 1);

        public ProtocolFamily ProtocolFamily => (ProtocolFamily)Bytes[Fields.ProtocolFamilyPosition];
        Span<byte> ProtocolFamilyByte => new Span<byte>(Bytes, Fields.ProtocolFamilyPosition, 1);

        public ReadOnlySpan<byte> SourceAddress => new Span<byte>(Bytes, Fields.SourceAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
        Span<byte> SourceAddressBytes => new Span<byte>(Bytes, Fields.SourceAddressPosition, 16);

        public ReadOnlySpan<byte> DestinationAddress => new Span<byte>(Bytes, Fields.DestinationAddressPosition, ProtocolFamily == ProtocolFamily.InterNetwork ? 4 : 16);
        Span<byte> DestinationAddressBytes => new Span<byte>(Bytes, Fields.DestinationAddressPosition, 16);

        public ushort SourcePort => BinaryPrimitives.ReadUInt16BigEndian(SourcePortBytes);
        Span<byte> SourcePortBytes => new Span<byte>(Bytes, Fields.SourcePortPosition, 2);

        public ushort DestinationPort => BinaryPrimitives.ReadUInt16BigEndian(DestinationPortBytes);
        Span<byte> DestinationPortBytes => new Span<byte>(Bytes, Fields.DestinationPortPosition, 2);

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

        public static FlowKey Create(ProtocolType protocol, IPAddress sourceAddress, int sourcePort, IPAddress destinationAddress, int destinationPort)
        {
            return Create((byte)protocol, sourceAddress.GetAddressBytes(), (ushort)sourcePort, destinationAddress.GetAddressBytes(), (ushort)destinationPort);
        }

        public static bool Compare(FlowKey f1, FlowKey f2)
        {
            return (f1 == f2)
                || (f1.FlowKeyHash == f2.FlowKeyHash) && Compare(f1.Bytes, f2.Bytes);
        }
        private static unsafe bool Compare(Span<byte> bytes1, Span<byte> bytes2)
        {
#if (LangVersion73)
            fixed (byte* ref1 = bytes1)
            fixed (byte* ref2 = bytes2)
#else
            fixed (byte* ref1 = &bytes1.GetPinnableReference())
            fixed (byte* ref2 = &bytes2.GetPinnableReference())
#endif
            {
                var ptr1 = (ulong*)ref1;
                var ptr2 = (ulong*)ref2;
                return ptr1[0] == ptr2[0] && ptr1[1] == ptr2[1] && ptr1[2] == ptr2[2]
                && ptr1[3] == ptr2[3] && ptr1[4] == ptr2[4];
            }
        }

        private static unsafe int GetHashCode(Span<byte> bytes)
        {
#if LangVersion73
            fixed (byte* bytePtr = bytes)
#else
            fixed (byte* bytePtr = &bytes.GetPinnableReference())
#endif
            {
                var intPtr = (int*)bytePtr;
                return (31 * (31 * (31 * (31 * (31 * (31 * (31 * (31 * (31 * intPtr[0] + intPtr[1]) + intPtr[2]) + intPtr[3]) + intPtr[4])
                    + intPtr[5]) + intPtr[6]) + intPtr[7]) + intPtr[8]) + intPtr[9]);
            }
        }

        static class Fields
        {
            internal const int ProtocolPosition = 0;
            internal const int ProtocolFamilyPosition = 1;
            internal const int SourceAddressPosition = 4;
            internal const int SourcePortPosition = 20;
            internal const int DestinationAddressPosition = 22;
            internal const int DestinationPortPosition = 38;
        }
    }
}
