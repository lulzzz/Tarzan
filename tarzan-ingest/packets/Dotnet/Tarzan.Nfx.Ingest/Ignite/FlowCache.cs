using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Resource;
using Netdx.PacketDecoders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class FlowCache
    {
        readonly ICacheEntryProcessor<PacketFlowKey, PacketStream, PacketStream, PacketStream> m_cacheEntryProcessor; 

        public ICache<PacketFlowKey, PacketStream> Cache { get; private set; }

        public FlowCache(IIgnite ignite)
        {
            this.Cache = GetCache(ignite);
            m_cacheEntryProcessor = new MergePacketStream();
        }

        public PacketStream Update(PacketFlowKey key, PacketStream value)
        {
            return Cache.Invoke(key, m_cacheEntryProcessor, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Using DataStreamer it is possible also to update existing entitites. To update existing flows in 
        /// the use <see cref="StreamTransformer{TK, TV, TArg, TRes}"/>.
        /// <code>
        /// flowCache.GetDataStreamer();
        /// 
        /// var transformer = new StreamTransformer<PacketFlowKey, PacketStream, PacketStream, PacketStream>(new MergePacketStream());
        /// </code>
        /// </remarks>
        public IDataStreamer<PacketFlowKey, PacketStream> GetDataStreamer()
        {
            return this.Cache.Ignite.GetDataStreamer<PacketFlowKey, PacketStream>(nameof(FlowCache));
        }

        public static ICache<PacketFlowKey, PacketStream> GetCache(IIgnite ignite) => ignite.GetOrCreateCache<PacketFlowKey, PacketStream>(nameof(FlowCache));

        public class MergePacketStream : ICacheEntryProcessor<PacketFlowKey, PacketStream, PacketStream, PacketStream>
        {   
            public PacketStream Process(IMutableCacheEntry<PacketFlowKey, PacketStream> entry, PacketStream arg)
            {
                entry.Value = PacketStream.Merge(entry.Value, arg);
                return entry.Value;
            }
        }
    }
}
