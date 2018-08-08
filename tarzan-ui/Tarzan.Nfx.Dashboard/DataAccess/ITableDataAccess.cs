using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tarzan.Nfx.Dashboard.DataAccess
{
    public interface ITableDataAccess<TRowType, TKey>
    {
        int Count();
        IEnumerable<TRowType> FetchRange(int start, int count);
        TRowType FetchItem(TKey key);
    }
}
