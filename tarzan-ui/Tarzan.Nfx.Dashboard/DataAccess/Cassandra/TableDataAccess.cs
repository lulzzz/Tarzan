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
        private Table<TRowType> m_table;
        private string m_tableName;
        private string m_keyProperty;

        private string m_selectTerm;

        public TableDataAccess(ISession session, string tableName, string keyProperty, params string[] selectFields)
        {
            m_session = session;
            m_mapper = new Mapper(m_session);
            m_table = new Table<TRowType>(m_session);
            m_tableName = tableName;
            m_keyProperty = keyProperty;
            m_selectTerm = selectFields.Length > 0 ? String.Join(',',selectFields) : "*";
        }
        

        public int Count()
        {
            return m_mapper.First<int>($"SELECT COUNT(*) FROM {m_tableName}");
        }

        public TRowType FetchItem(TKey key)
        {
            return m_mapper.SingleOrDefault<TRowType>($"SELECT {m_selectTerm} FROM {m_tableName} WHERE {m_keyProperty}=?", key.ToString());
        }

        public IEnumerable<TRowType> FetchRange(int start, int count)
        {
            return m_mapper.Fetch<TRowType>($"SELECT {m_selectTerm} FROM {m_tableName}").Skip(start).Take(count);
        }
    }
    public class TableDataAccess<TRowType, TKey1, TKey2> : ITableDataAccess<TRowType, TKey1, TKey2>
    {
        private ISession m_session;
        private IMapper m_mapper;
        private string m_tableName;
        private string m_keyProperty1;
        private string m_keyProperty2;
        private string m_selectTerm;
        public TableDataAccess(ISession session, string tableName, string keyProperty1, string keyProperty2, params string[] selectFields)
        {
            m_session = session;
            m_mapper = new Mapper(m_session);
            m_tableName = tableName;
            m_keyProperty1 = keyProperty1;
            m_keyProperty2 = keyProperty2;
            m_selectTerm = selectFields.Length > 0 ? String.Join(',', selectFields) : "*";
        }

        public int Count()
        {
            return m_mapper.First<int>($"SELECT COUNT(*) FROM {m_tableName}");
        }

        public TRowType FetchItem(TKey1 key1, TKey2 key2)
        {
            return m_mapper.SingleOrDefault<TRowType>($"SELECT {m_selectTerm} FROM {m_tableName} WHERE {m_keyProperty1}=? AND {m_keyProperty2}=?", key1.ToString(), key2.ToString());
        }

        public IEnumerable<TRowType> FetchRange(int start, int count)
        {
            return m_mapper.Fetch<TRowType>($"SELECT {m_selectTerm} FROM {m_tableName}").Skip(start).Take(count);
        }
    }
}
