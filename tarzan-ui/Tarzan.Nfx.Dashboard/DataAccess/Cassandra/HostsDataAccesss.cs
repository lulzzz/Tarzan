using Cassandra;
using Cassandra.Mapping;
using System.Collections.Generic;
using System.Linq;
namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    public class HostsDataAccesss : TableDataAccess<Nfx.Model.Host, string>
    {
        public HostsDataAccesss(ISession session) : base(session, "hosts", nameof(Nfx.Model.Host.Address).ToLowerInvariant())
        {
        }
    }
}
