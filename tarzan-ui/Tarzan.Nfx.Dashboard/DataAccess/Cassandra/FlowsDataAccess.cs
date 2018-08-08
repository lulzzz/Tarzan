using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cassandra;
using Cassandra.Mapping;
using Tarzan.Nfx.Model;

namespace Tarzan.UI.Server.DataAccess.Cassandra
{
    public class FlowsDataAccess : IFlowsDataAccess
    {
        ISession m_session;
        IMapper m_mapper;
        public FlowsDataAccess(Cluster cluster, string keyspace)
        {
            m_session = cluster.Connect(keyspace);
            m_mapper = new Mapper(m_session);
        }

        public IEnumerable<Flow> GetFlowRecords(int start = 0, int limit = Int32.MaxValue)        
        {
            // We do offset queries here, which is not efficient by nature. See following link for more details:
            // https://docs.datastax.com/en/developer/java-driver/3.2/manual/paging/
            return m_mapper.Fetch<Flow>("SELECT * FROM flows").Skip(start).Take(limit);
        }

        public Flow GetFlowRecord(Guid id)
        {
            return m_mapper.SingleOrDefault<Flow>("SELECT * FROM flows WHERE flowId=?", id.ToString());
        }

        public int RecordCount()
        {
            return m_mapper.First<int>("SELECT COUNT(*) FROM flows");
        }
    }
}