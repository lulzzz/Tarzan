using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public interface ICacheFactory<TKey, TValue>
    {
        IBinarySerializer Serializer { get; }

        CacheConfiguration CacheConfiguration { get; }

        ICache<TKey, TValue> GetCache();

        ICache<TKey, TValue> GetOrCreateCache();

    }
}