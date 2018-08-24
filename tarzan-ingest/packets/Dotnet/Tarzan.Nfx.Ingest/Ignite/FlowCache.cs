using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Datastream;
using Netdx.ConversationTracker;

namespace Tarzan.Nfx.Ingest.Ignite
{
    class FlowCache
    {
        public static ICache<string, PacketStream> GetCache(IIgnite ignite) => ignite.GetOrCreateCache<string, PacketStream>(nameof(FlowCache));

        CacheEntryProcessor m_cacheEntryProcessor = new CacheEntryProcessor();
        public ICache<string, PacketStream> Cache { get; private set; }

        public FlowCache(IIgnite ignite)
        {
            this.Cache = FlowCache.GetCache(ignite);
        }
        public async Task<PacketStream> UpdateAsync(FlowKey key, PacketStream value)
        {
            var strKey = $"{key.Protocol}:{key.SourceEndpoint}>{key.DestinationEndpoint}";
            return await Cache.InvokeAsync(strKey, m_cacheEntryProcessor, value);
        }

        class CacheEntryProcessor : ICacheEntryProcessor<string, PacketStream, PacketStream, PacketStream>
        {
            public PacketStream Process(IMutableCacheEntry<string, PacketStream> entry, PacketStream arg)
            {
                entry.Value = entry.Exists ? PacketStream.Merge(entry.Value, arg) : arg;
                return entry.Value;
            }
        }

        internal IDataStreamer<string,PacketStream> GetDataStreamer()
        {
            return this.Cache.Ignite.GetDataStreamer<string, PacketStream>(nameof(FlowCache));
        }
    }
}
