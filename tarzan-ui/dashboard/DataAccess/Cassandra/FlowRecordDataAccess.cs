using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cassandra;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess.Cassandra
{
    public class FlowRecordDataAccess : IFlowRecordDataAccess
    {
        ISession m_session;
        public FlowRecordDataAccess(Cluster cluster, string keyspace)
        {
            m_session = cluster.Connect(keyspace);
        }

        public IEnumerable<FlowRecord> GetFlowRecords(int start = 0, int limit = Int32.MaxValue)        
        {
            var rs = m_session.Execute("select * from flows").Skip(start).Take(limit);
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

        public int RecordCount()
        {
            throw new NotImplementedException();
        }
    }
}