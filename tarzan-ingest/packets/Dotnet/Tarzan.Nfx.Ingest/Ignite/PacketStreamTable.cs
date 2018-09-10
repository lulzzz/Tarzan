using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;

namespace Tarzan.Nfx.Ingest.Ignite
{
    class PacketStreamTable : ICacheFactory<string, PacketStream>
    {
        private IIgnite m_ignite;
        public PacketStreamTable(IIgnite m_ignite)
        {
            this.m_ignite = m_ignite;
        }

        public ICache<string, PacketStream> GetCache()
        {
            return this.m_ignite.GetCache<string, PacketStream>(nameof(PacketStreamTable));
        }

        public ICache<string, PacketStream> GetOrCreateCache()
        {
            return this.m_ignite.GetOrCreateCache<string, PacketStream>(nameof(PacketStreamTable));
        }
    }
}
