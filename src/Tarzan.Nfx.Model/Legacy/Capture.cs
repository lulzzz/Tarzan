using System;
using System.Net;
using Cassandra.Mapping;

namespace Tarzan.Nfx.Model
{

    /// <summary>
    /// Represents a single flow record.
    /// </summary>
    public partial class Capture
    {
        public static Map<Capture> Mapping =>
            new Map<Capture>()
            .TableName(Pluralizer.Pluralize(nameof(Capture)))
            .PartitionKey(c => c.Uid)
            .Column(c => c.__isset, cc=> cc.Ignore());       
    }
}