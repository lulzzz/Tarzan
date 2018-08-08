using System;
using System.Net;
using Cassandra.Mapping;
using Newtonsoft.Json;

namespace Tarzan.Nfx.Model
{
    /// <summary>
    /// Represents a single flow record.
    /// </summary>
    public partial class Flow
    {
        [JsonIgnore]
        public IPAddress SourceIpAddress => IPAddress.Parse(this.SourceAddress);
        [JsonIgnore]
        public IPAddress DestinationIpAddress => IPAddress.Parse(this.DestinationAddress);

        public static Map<Flow> Mapping =>
            new Map<Flow>()
                .TableName("flows")
                .PartitionKey("protocol", "sourceAddress", "sourcePort", "destinationAddress", "destinationPort")
                .ClusteringKey(x => x.FlowId)
                .Column(f=>f.FlowId, cc => cc.WithSecondaryIndex())
                .Column(f => f.SourceIpAddress, cc => cc.Ignore())
                .Column(f => f.DestinationIpAddress, cc => cc.Ignore())
                .Column(f => f.__isset, cc => cc.Ignore());
    }
}