using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    public partial class Service
    {
        public static Map<Service> Mapping =>
            new Map<Service>()
                .TableName(Pluralizer.Pluralize(nameof(Service)))
                .PartitionKey(nameof(Service.ServiceName))
                .Column(f => f.__isset, cc => cc.Ignore());
    }
}
