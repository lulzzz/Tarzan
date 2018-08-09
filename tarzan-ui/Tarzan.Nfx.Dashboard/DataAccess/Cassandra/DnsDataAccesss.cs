using Cassandra;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    internal class DnsDataAccesss : TableDataAccess<Dns, Guid, string>
    {
        public DnsDataAccesss(ISession session) : base(session, "dns", nameof(Dns.FlowId).ToLowerInvariant(), nameof(Dns.DnsId).ToLowerInvariant())
        {
        }
    }
}