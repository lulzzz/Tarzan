using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using Tarzan.UI.Server.DataAccess;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess.Cassandra
{
    public class CaptureDataAccess : ICaptureDataAccess
    {
        ISession m_session;
        IMapper m_mapper;

        public CaptureDataAccess(Cluster cluster, string keyspace)
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
