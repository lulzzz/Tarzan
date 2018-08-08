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
                .TableName("services")
                .PartitionKey("name")
                .Column(f => f.__isset, cc => cc.Ignore());
    }
}
