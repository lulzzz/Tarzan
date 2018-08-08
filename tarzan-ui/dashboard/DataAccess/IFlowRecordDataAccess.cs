using System;
using System.Collections.Generic;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess
{
    public interface IFlowRecordDataAccess
    {
        IEnumerable<Flow> GetFlowRecords(int start = 0, int length = int.MaxValue);
        Flow GetFlowRecord(Guid id);
        int RecordCount();
    }
}