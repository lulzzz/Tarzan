using Cassandra;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    internal class DnsDataAccesss : TableDataAccess<DnsInfo, Guid, string>
    {
        public DnsDataAccesss(ISession session) : base(session, nameof(DnsInfo), nameof(DnsInfo.FlowId).ToLowerInvariant(), nameof(DnsInfo.DnsId).ToLowerInvariant())
        {
        }
    }
}