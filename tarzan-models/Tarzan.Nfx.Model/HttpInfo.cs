using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    public partial class HttpInfo
    {
        public static Map<HttpInfo> Mapping =>
            new Map<HttpInfo>()
                .TableName(nameof(HttpInfo))
                .PartitionKey(x => x.FlowId)
                .ClusteringKey(x => x.TransactionId)
                .Column(f => f.__isset, cc => cc.Ignore());
    }
}
