using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    public partial class DnsInfo
    {
        public static Map<DnsInfo> Mapping =>
            new Map<DnsInfo>()
                .TableName(nameof(DnsInfo))
                .PartitionKey(c=>c.FlowId)
                .ClusteringKey(c=>c.DnsId)
                .Column(f => f.__isset, cc => cc.Ignore())
                .Column(f => f.DnsQuery, cc => cc.WithSecondaryIndex());
    }
}
