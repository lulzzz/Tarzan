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
                .PartitionKey(x => x.FlowUid)
                .Column(f => f.SourceIpAddress, cc => cc.Ignore())
                .Column(f => f.DestinationIpAddress, cc => cc.Ignore())
                .Column(f => f.__isset, cc => cc.Ignore())
                .Column(f => f.Protocol, cc => cc.WithSecondaryIndex())
                .Column(f => f.SourceAddress, cc => cc.WithSecondaryIndex())
                .Column(f => f.SourcePort, cc => cc.WithSecondaryIndex())
                .Column(f => f.DestinationAddress, cc => cc.WithSecondaryIndex())
                .Column(f => f.DestinationPort, cc => cc.WithSecondaryIndex());

        public string ObjectName => $"urn:aff4:flow/{this.FlowUid}";
        [JsonIgnore]
        public IPAddress SourceIpAddress => IPAddress.Parse(this.SourceAddress);
        [JsonIgnore]
        public IPAddress DestinationIpAddress => IPAddress.Parse(this.DestinationAddress);



    }
}