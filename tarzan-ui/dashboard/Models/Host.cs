using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tarzan.UI.Server.Models
{
    public partial class Host
    {
        public static Map<Host> Mapping =>
            new Map<Host>()
                .TableName("hosts")
                .PartitionKey("address");
    }
}
