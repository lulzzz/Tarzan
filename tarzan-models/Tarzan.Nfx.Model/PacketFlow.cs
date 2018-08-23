using System;
using System.Net;
using Cassandra.Mapping;
using Newtonsoft.Json;

namespace Tarzan.Nfx.Model
{
    /// <summary>
    /// Represents a single flow record.
    /// </summary>
    public partial class PacketFlow
    {
        public static Map<PacketFlow> Mapping =>
            new Map<PacketFlow>()
                .TableName(Pluralizer.Pluralize(nameof(PacketFlow)))
                .PartitionKey(x => x.Uid)
                .Column(f => f.SourceIpAddress, cc => cc.Ignore())
                .Column(f => f.DestinationIpAddress, cc => cc.Ignore())
                .Column(f => f.__isset, cc => cc.Ignore())
                .Column(f => f.Protocol, cc => cc.WithSecondaryIndex())
                .Column(f => f.SourceAddress, cc => cc.WithSecondaryIndex())
                .Column(f => f.SourcePort, cc => cc.WithSecondaryIndex())
                .Column(f => f.DestinationAddress, cc => cc.WithSecondaryIndex())
                .Column(f => f.DestinationPort, cc => cc.WithSecondaryIndex());

        public string ObjectName => $"urn:aff4:flow/{this.Uid}";
        [JsonIgnore]
        public IPAddress SourceIpAddress => IPAddress.Parse(this.SourceAddress);
        [JsonIgnore]
        public IPAddress DestinationIpAddress => IPAddress.Parse(this.DestinationAddress);


        // The requirements for these types of UUIDs are as follows:
        //
        // The UUIDs generated at different times from the same name in the
        // same namespace MUST be equal.
        // The UUIDs generated from two different names in the same namespace
        // should be different(with very high probability).
        // The UUIDs generated from the same name in two different namespaces
        // should be different with(very high probability).
        // If two UUIDs that were generated from names are equal, then they
        // were generated from the same name in the same namespace (with very
        // high probability).
        public static Guid NewUid(string protocol, IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint, long firstSeen)
        {
            var protoHash = protocol.ToUpperInvariant().GetHashCode();
            var dstHash = BitConverter.GetBytes(destinationEndpoint.GetHashCode() ^ protoHash);
            var srcHash = BitConverter.GetBytes(sourceEndpoint.GetHashCode() ^ protoHash);
            var timeHash = BitConverter.GetBytes(firstSeen);
            var buffer = new byte[16];
            timeHash.CopyTo(buffer, 0);
            srcHash.CopyTo(buffer, 8);
            dstHash.CopyTo(buffer, 12);
            var guid = new Guid(buffer);
            return guid;
        }
    }
}