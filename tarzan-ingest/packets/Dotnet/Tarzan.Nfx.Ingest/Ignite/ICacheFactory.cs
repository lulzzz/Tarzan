using Apache.Ignite.Core.Cache;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public interface ICacheFactory<TKey, TValue>
    {
        ICache<TKey, TValue> GetCache();
        ICache<TKey, TValue> GetOrCreateCache();
    }
}