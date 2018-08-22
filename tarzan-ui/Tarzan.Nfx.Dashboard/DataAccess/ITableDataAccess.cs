using Cassandra.Data.Linq;
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
    public interface ITableDataAccess<TRowType, TKey1, TKey2>
    {
        int Count();
        IEnumerable<TRowType> FetchRange(int start, int count);
        TRowType FetchItem(TKey1 key1, TKey2 key2);
    }
    public interface ITableDataAccess<TRowType, TKey1, TKey2, TKey3>
    {
        int Count();
        IEnumerable<TRowType> FetchRange(int start, int count);
        TRowType FetchItem(TKey1 key1, TKey2 key2, TKey3 key3);
    }
}
