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
                .TableName(Pluralizer.Pluralize(nameof(Host)))
                .PartitionKey(nameof(Host.Address))
                .Column(f => f.__isset, cc => cc.Ignore());

        public string ObjectName => $"urn:aff4:host/{this.Address}";
    }
}
