using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    public class FlowsDataAccess : TableDataAccess<PacketFlow, Guid>
    {
        public FlowsDataAccess(ISession session) : base(session, "flows", nameof(PacketFlow.FlowId).ToLowerInvariant())
        {
        }  
    }
}