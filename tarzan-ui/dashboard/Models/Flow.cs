using System;
using System.Net;
using Cassandra.Mapping;
using Newtonsoft.Json;

namespace Tarzan.UI.Server.Models
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
                .Column<IPAddress>(f => f.SourceIpAddress, cc => cc.Ignore())
                .Column<IPAddress>(f => f.DestinationIpAddress, cc => cc.Ignore());
    }
}