using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    public class TableDataAccess<TRowType, TKey> : ITableDataAccess<TRowType, TKey>
    {
        private ISession m_session;
        private IMapper m_mapper;
        private string m_tableName;
        private string m_keyProperty;
        public TableDataAccess(ISession session, string tableName, string keyProperty)
        {
            m_session = session;
            m_mapper = new Mapper(m_session);
            m_tableName = tableName;
            m_keyProperty = keyProperty;
        }

        public int Count()
        {
            return m_mapper.First<int>($"SELECT COUNT(*) FROM {m_tableName}");
        }

        public TRowType FetchItem(TKey key)
        {
            return m_mapper.SingleOrDefault<TRowType>($"SELECT * FROM {m_tableName} WHERE {m_keyProperty}=?", key.ToString());
        }

        public IEnumerable<TRowType> FetchRange(int start, int count)
        {
            return m_mapper.Fetch<TRowType>($"SELECT * FROM {m_tableName}").Skip(start).Take(count);
        }
    }
}
