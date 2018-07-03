using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cassandra;
namespace Tarzan.UI.Server.Models
{
    public class FlowRecordDataAccess 
    {
        ISession m_session;
        public FlowRecordDataAccess(IPEndPoint cassandraDb, string keyspace)
        {
            var cluster = Cluster.Builder()
            .AddContactPoints(cassandraDb)
            .Build();
            m_session = cluster.Connect(keyspace);
        }

        public IEnumerable<FlowRecord> GetAllFlowRecords(int limit = Int32.MaxValue)        
        {
            var rs = m_session.Execute("select * from flows").Take(limit);
            foreach (var row in rs)
            {
                yield return new FlowRecord(row);
            }
        }

        public FlowRecord GetFlowRecord(int id)
        {            
            var rs = m_session.Execute($"select * from flows where id={id}");
            var row = rs.FirstOrDefault();
            return (row!=null) ? new FlowRecord(row) : null;
        }
    }
}