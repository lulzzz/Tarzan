using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess.Mock
{
    public class FlowRecordDataAccess : IFlowRecordDataAccess
    {
        public FlowRecordDataAccess()
        {
        }

        public IEnumerable<FlowRecord> GetAllFlowRecords(int limit = Int32.MaxValue)        
        {
            throw new NotImplementedException();    
        }

        public FlowRecord GetFlowRecord(int id)
        {
            throw new NotImplementedException();
        }
    }
}