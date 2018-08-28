using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Resource;
using Netdx.PacketDecoders;
using System;
using System.Threading.Tasks;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class FlowCache
    {
        readonly ICacheEntryProcessor<PacketFlowKey, PacketStream, PacketStream, PacketStream> m_cacheEntryProcessor = new MergePacketStream();

        class MergePacketStream : ICacheEntryProcessor<PacketFlowKey, PacketStream, PacketStream, PacketStream>
        {
            public PacketStream Process(IMutableCacheEntry<PacketFlowKey, PacketStream> entry, PacketStream arg)
            {
                entry.Value = PacketStream.Merge(entry.Value, arg);
                return entry.Value;
            }
        }

        public ICache<PacketFlowKey, PacketStream> Cache { get; private set; }

        public FlowCache(IIgnite ignite)
        {
            this.Cache = GetCache(ignite);
        }

        public async Task<PacketStream> UpdateAsync(PacketFlowKey key, PacketStream value)
        {
            return await Cache.InvokeAsync(key, m_cacheEntryProcessor, value);
        }

        public IDataStreamer<PacketFlowKey, PacketStream> GetDataStreamer()
        {
            return this.Cache.Ignite.GetDataStreamer<PacketFlowKey, PacketStream>(nameof(FlowCache));
        }

        public static ICache<PacketFlowKey, PacketStream> GetCache(IIgnite ignite) => ignite.GetOrCreateCache<PacketFlowKey, PacketStream>(nameof(FlowCache));
    }
}
