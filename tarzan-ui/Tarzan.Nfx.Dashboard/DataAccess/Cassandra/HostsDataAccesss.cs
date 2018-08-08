using Cassandra;
using Cassandra.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Tarzan.UI.Server.DataAccess.Cassandra
{
    public class HostsDataAccesss : IHostsDataAccess
    {
        ISession m_session;
        IMapper m_mapper;

        public HostsDataAccesss(Cluster cluster, string keyspace)
        {
            m_session = cluster.Connect(keyspace);
            m_mapper = new Mapper(m_session);
        }

        public int Count()
        {
            return m_mapper.First<int>("SELECT COUNT(*) FROM hosts");
        }

        public IEnumerable<Tarzan.Nfx.Model.Host> Fetch(int start, int count)
        {
            return m_mapper.Fetch<Tarzan.Nfx.Model.Host>("SELECT * FROM hosts").Skip(start).Take(count);
        }

        public Tarzan.Nfx.Model.Host FetchByAddress(string address)
        {
            return m_mapper.SingleOrDefault<Tarzan.Nfx.Model.Host>("SELECT * FROM hosts WHERE address=?", address);
        }
    }
}
