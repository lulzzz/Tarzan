using Apache.Ignite.Core.Cache;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class MergePacketStreamProcessor : ICacheEntryProcessor<FlowKey, PacketStream, PacketStream, PacketStream>
    {
        public PacketStream Process(IMutableCacheEntry<FlowKey, PacketStream> entry, PacketStream arg)
        {
            entry.Value = PacketStream.Merge(entry.Value, arg);
            return entry.Value;
        }
    }

}
