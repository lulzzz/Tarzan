using Apache.Ignite.Core.Cache;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class MergePacketStreamProcessor : ICacheEntryProcessor<FlowKey, PacketStream, PacketStream, PacketStream>
    {
        public PacketStream Process(IMutableCacheEntry<FlowKey, PacketStream> entry, PacketStream arg)
        {
            if (entry.Exists)
            {
                entry.Value = PacketStream.Merge(entry.Value, arg);
            }
            else
            {
                entry.Value = arg;
            }
            return null;
        }
    }

}
