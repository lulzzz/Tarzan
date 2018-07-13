using System.Collections.Generic;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess
{
    public interface IFlowRecordDataAccess
    {
        IEnumerable<FlowRecord> GetAllFlowRecords(int start = 0, int length = int.MaxValue);
        FlowRecord GetFlowRecord(int id);
        int RecordCount();
    }
}