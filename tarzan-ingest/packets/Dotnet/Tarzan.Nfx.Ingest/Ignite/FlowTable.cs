using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class FlowTable : ICacheFactory<FlowKey, PacketStream>
    {
        public static readonly IBinarySerializer m_serializer = new FlowTableSerializer();
        public static readonly CacheConfiguration m_cacheConfiguration = new CacheConfiguration(nameof(FlowTable))
        {
            CacheMode = CacheMode.Partitioned,
            Backups = 0,
            QueryEntities = new[]
            {
                new QueryEntity(typeof(PacketStream))
                {
                    Fields = new []
                    {
                        new QueryField(nameof(PacketStream.FlowUid), typeof(string)),
                        new QueryField(nameof(PacketStream.Protocol), typeof(short)),
                        new QueryField(nameof(PacketStream.SourceAddress), typeof(string)),
                        new QueryField(nameof(PacketStream.SourcePort), typeof(int)),
                        new QueryField(nameof(PacketStream.DestinationAddress), typeof(string)),
                        new QueryField(nameof(PacketStream.DestinationPort), typeof(int)),
                        new QueryField(nameof(PacketStream.ServiceName), typeof(string)),
                        new QueryField(nameof(PacketStream.FirstSeen), typeof(long)),
                        new QueryField(nameof(PacketStream.LastSeen), typeof(long)),
                        new QueryField(nameof(PacketStream.Packets), typeof(int)),
                        new QueryField(nameof(PacketStream.Octets), typeof(long)),
                    }
                }
            }
        };
        private IIgnite m_ignite;

        public FlowTable(IIgnite m_ignite)
        {
            this.m_ignite = m_ignite;
        }

        public IBinarySerializer Serializer => throw new NotImplementedException();

        public CacheConfiguration CacheConfiguration => m_cacheConfiguration;

        public ICache<FlowKey, PacketStream> GetCache()
        {
            return this.m_ignite.GetCache<FlowKey, PacketStream>(nameof(FlowTable));
        }

        public ICache<FlowKey, PacketStream> GetOrCreateCache()
        {
            return this.m_ignite.GetOrCreateCache<FlowKey, PacketStream>(nameof(FlowTable));
        }
    }
}
