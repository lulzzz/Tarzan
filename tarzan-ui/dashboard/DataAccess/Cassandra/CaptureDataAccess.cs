using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Tarzan.UI.Server.DataAccess;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess.Cassandra
{
    public class CaptureDataAccess : ICaptureDataAccess
    {
        ISession m_session;

        public CaptureDataAccess(Cluster cluster, string keyspace)
        {
            m_session = cluster.Connect(keyspace);
        }

        public IEnumerable<Capture> GetAllCaptures(int limit = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public Capture GetCapture(int id)
        {
            throw new NotImplementedException();
        }
    }
}
