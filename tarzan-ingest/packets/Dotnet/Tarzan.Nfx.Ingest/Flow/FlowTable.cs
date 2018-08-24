using System.Collections.Generic;
using System.Text;
using Netdx.ConversationTracker;
using PacketDotNet;
using IPEndPoint = System.Net.IPEndPoint;
using SharpPcap;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

namespace Tarzan.Nfx.Ingest
{
    class FlowTable : IFlowTable<FlowKey, PacketStream>
    {
        ConcurrentDictionary<FlowKey, PacketStream> m_table = new ConcurrentDictionary<FlowKey, PacketStream>();

        public IEnumerable<KeyValuePair<FlowKey, PacketStream>> Entries => m_table;


        public int Count => m_table.Count;

        public PacketStream Delete(FlowKey key)
        {
                m_table.Remove(key, out var record);
                return record;
        }

        public bool Exists(FlowKey key)
        {
            return m_table.ContainsKey(key);
        }

        public void FlushAll()
        {
                m_table.Clear();
        }

        public PacketStream Get(FlowKey key)
        {
            return m_table.GetValueOrDefault(key);
        }

        public PacketStream Merge(FlowKey key, PacketStream value)
        {
            return m_table.AddOrUpdate(key, value, (k, v) => PacketStream.Merge(v, value));
        }

        public void Put(FlowKey key, PacketStream value)
        {
            m_table[key] = value;
        }
    }
}
