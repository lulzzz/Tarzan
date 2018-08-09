using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    public partial class Dns
    {
        public static Map<Dns> Mapping =>
            new Map<Dns>()
                .TableName("dns")
                .PartitionKey("flowId")
                .ClusteringKey("dnsId")
                .Column(f => f.__isset, cc => cc.Ignore())
                .Column(f => f.DnsQuery, cc => cc.WithSecondaryIndex());
    }
}
