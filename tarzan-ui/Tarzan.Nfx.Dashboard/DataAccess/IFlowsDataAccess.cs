using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.UI.Server.DataAccess
{
    public interface IFlowsDataAccess
    {
        IEnumerable<Flow> GetFlowRecords(int start = 0, int length = int.MaxValue);
        Flow GetFlowRecord(Guid id);
        int RecordCount();
    }
}