using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public interface ITableConfigurationProvider
    {
        BinaryTypeConfiguration TypeConfiguration { get; }
        CacheConfiguration CacheConfiguration { get; }
    }
}