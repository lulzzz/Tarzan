using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Datastream;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;

namespace Tarzan.Nfx.Ingest.Ignite
{
    class FlowCache
    {
        public static ICache<PacketFlowKey, PacketStream> GetCache(IIgnite ignite) => ignite.GetOrCreateCache<PacketFlowKey, PacketStream>(nameof(FlowCache));

        CacheEntryProcessor m_cacheEntryProcessor = new CacheEntryProcessor();
        public ICache<PacketFlowKey, PacketStream> Cache { get; private set; }

        public FlowCache(IIgnite ignite)
        {
            this.Cache = FlowCache.GetCache(ignite);
        }
        public async Task<PacketStream> UpdateAsync(PacketFlowKey key, PacketStream value)
        {
            return await Cache.InvokeAsync(key, m_cacheEntryProcessor, value);
        }

        class CacheEntryProcessor : ICacheEntryProcessor<PacketFlowKey, PacketStream, PacketStream, PacketStream>
        {
            public PacketStream Process(IMutableCacheEntry<PacketFlowKey, PacketStream> entry, PacketStream arg)
            {
                entry.Value = entry.Exists ? PacketStream.Merge(entry.Value, arg) : arg;
                return entry.Value;
            }
        }

        internal IDataStreamer<PacketFlowKey, PacketStream> GetDataStreamer()
        {
            return this.Cache.Ignite.GetDataStreamer<PacketFlowKey, PacketStream>(nameof(FlowCache));
        }
    }
}
