using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tarzan.Nfx.Model
{
    public partial class Host
    {
        public static Map<Host> Mapping =>
            new Map<Host>()
                .TableName("hosts")
                .PartitionKey("address")
                .Column(f => f.__isset, cc => cc.Ignore());
    }
}
