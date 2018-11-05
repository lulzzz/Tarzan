using Apache.Ignite.Core.Cache;
using System.Collections.Generic;

namespace Tarzan.Nfx.Ignite
{
    public interface IReferencedCache<TRefKey,TKey,TValue>
    {
        IEnumerable<ICacheEntry<TKey,TValue>> GetItems(TRefKey refkey);
    }
}
