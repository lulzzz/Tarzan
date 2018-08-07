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
            m_session.UserDefinedTypes.Define(UdtMap.For<FlowRecord.IpEndPoint>());
        }

        public IEnumerable<FlowRecord> GetFlowRecords(int start = 0, int limit = Int32.MaxValue)        
        {
            var rs = m_session.Execute("select * from flows").Skip(start).Take(limit);
            foreach (var row in rs)
            {
                yield return new FlowRecord(row);
            }
        }

        public FlowRecord GetFlowRecord(Guid id)
        {            
            var rs = m_session.Execute($"select * from flows where flowid={id}");
            var row = rs.FirstOrDefault();
            return (row!=null) ? new FlowRecord(row) : null;
        }

        public int RecordCount()
        {
            var rs = m_session.Execute($"select count(*) from flows");
            var row = rs.FirstOrDefault();
            return (int)row.GetValue<long>("count");
        }
    }
}