using System.Collections.Generic;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess
{
    public interface IFlowRecordDataAccess
    {
        IEnumerable<FlowRecord> GetAllFlowRecords(int limit = int.MaxValue);
        FlowRecord GetFlowRecord(int id);
    }
}