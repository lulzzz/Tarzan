using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using Tarzan.UI.Server.DataAccess;
using Tarzan.Nfx.Model;

namespace Tarzan.UI.Server.DataAccess.Cassandra
{
    public class CapturesDataAccess : ICapturesDataAccess
    {
        ISession m_session;
        IMapper m_mapper;

        public CapturesDataAccess(Cluster cluster, string keyspace)
        {
            m_session = cluster.Connect(keyspace);
            m_mapper = new Mapper(m_session);
        }

        public Capture GetCapture(Guid id)
        {
            return m_mapper.SingleOrDefault<Capture>("FROM captures WHERE id=?", id);
        }

        public IEnumerable<Capture> GetCaptures(int start = 0, int limit = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public int CaptureCount()
        {
            throw new NotImplementedException();
        }
    }
}
