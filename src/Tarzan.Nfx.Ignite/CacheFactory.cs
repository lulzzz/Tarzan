using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Client.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ignite
{
    public class CacheFactory
    {
        public static ICache<FrameKey, FrameData> GetOrCreateFrameCache(IIgnite ignite, string cacheName)
        {
            return GetOrCreateCache<FrameKey, FrameData>(ignite, cacheName);
            /*
            var cacheCfg = new CacheConfiguration()
            {
                Name = cacheName,
                CacheMode = CacheMode.Partitioned,
                GroupName = typeof(FrameData).FullName,
                QueryEntities = new[]
                {
                    new QueryEntity
                    {
                        KeyType = typeof(FrameKey),
                        ValueType = typeof(FrameData),
                    }
                }
            };

            return ignite.GetOrCreateCache<FrameKey, FrameData>(cacheCfg);
            */
        }

        public static ICacheClient<FrameKey, FrameData> GetOrCreateFrameCache(IIgniteClient ignite, string cacheName)
        {
            return GetOrCreateCache<FrameKey, FrameData>(ignite, cacheName);
            /*
            var cacheCfg = new CacheClientConfiguration()
            {
                Name = cacheName,
                CacheMode = CacheMode.Partitioned,
                GroupName = typeof(FrameData).FullName,
                QueryEntities = new[]
                {
                    new QueryEntity
                    {
                        KeyType = typeof(FrameKey),
                        ValueType = typeof(FrameData),
                    }
                }
            };

            return ignite.GetOrCreateCache<FrameKey, FrameData>(cacheCfg);
            */
        }

        public static ICache<FlowKey, FlowData> GetOrCreateFlowCache(IIgnite ignite, string cacheName)
        {

            return GetOrCreateCache<FlowKey, FlowData>(ignite, cacheName);
        }
        public static ICacheClient<FlowKey, FlowData> GetOrCreateFlowCache(IIgniteClient ignite, string cacheName)
        {
            return GetOrCreateCache<FlowKey, FlowData>(ignite, cacheName);
        }

        public static ICache<TKey, TData> GetOrCreateCache<TKey, TData>(IIgnite ignite, string cacheName)
        {
            var cacheCfg = new CacheConfiguration()
            {
                Name = cacheName,
                CacheMode = CacheMode.Partitioned,
                GroupName = typeof(TData).FullName,
                QueryEntities = new[]
                {
                    new QueryEntity
                    {
                        KeyType = typeof(TKey),
                        ValueType = typeof(TData),
                    }
                }
            };

            return ignite.GetOrCreateCache<TKey, TData>(cacheCfg);
        }
        public static ICacheClient<TKey, TData> GetOrCreateCache<TKey, TData>(IIgniteClient ignite, string cacheName)
        {
            var cacheCfg = new CacheClientConfiguration()
            {
                Name = cacheName,
                CacheMode = CacheMode.Partitioned,
                GroupName = typeof(TData).FullName,
                QueryEntities = new[]
                {
                    new QueryEntity
                    {
                        KeyType = typeof(TKey),
                        ValueType = typeof(TData),
                    }
                }
            };

            return ignite.GetOrCreateCache<TKey, TData>(cacheCfg);
        }

        public static IReferencedCache<FlowKey,FrameKey,FrameData> GetFrameCacheCollection(IIgnite ignite, IEnumerable<string> frameCacheNames)
        {
            return new FrameCacheCollection(ignite, frameCacheNames);
        }
    }
}
