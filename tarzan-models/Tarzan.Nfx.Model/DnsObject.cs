using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    public partial class DnsObject
    {
        public static Map<DnsObject> Mapping =>
            new Map<DnsObject>()
                .TableName(Pluralizer.Pluralize(nameof(DnsObject)))
                .PartitionKey(c=>c.FlowUid)
                .ClusteringKey(c=>c.TransactionId)
                .Column(f => f.__isset, cc => cc.Ignore())
                .Column(f => f.DnsQuery, cc => cc.WithSecondaryIndex());

        public string ObjectName => $"urn:aff4:dns/{this.FlowUid}/{this.TransactionId}";
    }
}
