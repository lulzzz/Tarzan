using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Text;
namespace Tarzan.Nfx.Model
{
    public partial class HttpObject
    {
        public static Map<HttpObject> Mapping =>
            new Map<HttpObject>()
                .TableName(Pluralizer.Pluralize(nameof(HttpObject)))
                .PartitionKey(x => x.FlowUid)
                .ClusteringKey(x => x.ObjectIndex)
                .Column(f => f.__isset, cc => cc.Ignore());

        public string ObjectName => $"urn:aff4:http/{this.FlowUid}/{this.ObjectIndex}";
    }
}
