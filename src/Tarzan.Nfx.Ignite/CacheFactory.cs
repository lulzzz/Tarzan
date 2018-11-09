using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Client.Cache;
using Apache.Ignite.Core.Cluster;
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
                    },
                }
            };
            cacheCfg.KeyConfiguration = new CacheKeyConfiguration[]
                {
                    new CacheKeyConfiguration { TypeName =  typeof(FrameKey).FullName, AffinityKeyFieldName = nameof(FrameKey.FlowKeyHash)}
                };

            var cache = ignite.GetOrCreateCache<FrameKey, FrameData>(cacheCfg);
            return cache;
        }

        public static ICacheClient<FrameKey, FrameData> GetOrCreateFrameCache(IIgniteClient ignite, string cacheName)
        {
            void Extend(CacheClientConfiguration cfg)
            {
                cfg.KeyConfiguration = new CacheKeyConfiguration[]
                {
                    new CacheKeyConfiguration { TypeName =  typeof(FrameKey).FullName, AffinityKeyFieldName = nameof(FrameKey.FlowKeyHash)}
                };
            }
            return GetOrCreateCache<FrameKey, FrameData>(ignite, cacheName, Extend);           
        }

        public static ICache<FlowKey, FlowData> GetOrCreateFlowCache(IIgnite ignite, string cacheName)
        {

            return GetOrCreateCache<FlowKey, FlowData>(ignite, cacheName);
        }
        public static ICacheClient<FlowKey, FlowData> GetOrCreateFlowCache(IIgniteClient ignite, string cacheName)
        {
            return GetOrCreateCache<FlowKey, FlowData>(ignite, cacheName);
        }

        public static ICache<TKey, TData> GetOrCreateCache<TKey, TData>(IIgnite ignite, string cacheName, Action<CacheConfiguration> extendConfigurationAction = null)
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
                    },                    
                }
            };
            extendConfigurationAction?.Invoke(cacheCfg);
            var cache = ignite.GetOrCreateCache<TKey, TData>(cacheCfg);
            return cache;
        }
        public static ICacheClient<TKey, TData> GetOrCreateCache<TKey, TData>(IIgniteClient ignite, string cacheName, Action<CacheClientConfiguration> extendConfigurationAction = null)
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
            extendConfigurationAction?.Invoke(cacheCfg);
            return ignite.GetOrCreateCache<TKey, TData>(cacheCfg);
        }

        public static IReferencedCache<FlowKey,FrameKey,FrameData> GetFrameCacheCollection(IIgnite ignite, IEnumerable<string> frameCacheNames)
        {
            return new FrameCacheCollection(ignite, frameCacheNames);
        }
    }
}
