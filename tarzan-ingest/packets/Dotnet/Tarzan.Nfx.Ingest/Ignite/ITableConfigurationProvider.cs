using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using System;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public interface ITableConfigurationProvider
    {
        BinaryTypeConfiguration TypeConfiguration { get; }
        CacheConfiguration CacheConfiguration { get; }
        Type ObjectType { get; } 
    }
}