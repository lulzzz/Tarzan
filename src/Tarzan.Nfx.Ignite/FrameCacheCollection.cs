using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;

namespace Tarzan.Nfx.Ignite
{
    /// <summary>
    /// It represents a collection of frame caches and provides 
    /// operations for accessing individual frames or group of frames.
    /// </summary>
    public class FrameCacheCollection
    {
        private readonly IIgnite m_ignite;
        /// <summary>
        /// Names of frame caches.
        /// </summary>
        private string[] m_frameCacheNames;
        /// <summary>
        /// An array of compiled queries for all caches.
        /// </summary>
        private Func<int, IQueryCursor<ICacheEntry<FrameKey, FrameData>>>[] m_compiledQueries;

        /// <summary>
        /// Creates a new frame provider that searches for frames in the specified collection of caches.
        /// </summary>
        /// <param name="ignite">An instance of the ignite service used to access the cache objects.</param>
        /// <param name="frameCacheNames">Collection of frame cache names.</param>
        /// <param name="local">If set, it says that only locally avaialable object can be accessed.</param>
        public FrameCacheCollection(IIgnite ignite, IEnumerable<string> frameCacheNames, bool local=false)
        {
            m_ignite = ignite;
            m_frameCacheNames = frameCacheNames.ToArray();
            m_compiledQueries = new Func<int, IQueryCursor<ICacheEntry<FrameKey, FrameData>>>[m_frameCacheNames.Length];
            for(int i = 0; i < m_frameCacheNames.Length; i++)
            {
                var queryable = CacheFactory.GetOrCreateFrameCache(m_ignite, m_frameCacheNames[i]).AsCacheQueryable(local: local);
                m_compiledQueries[i] = CompiledQuery.Compile((int hashCode) => queryable.Where(x => x.Key.FlowKeyHash == hashCode));
            }
        }

        public IEnumerable<ICacheEntry<FrameKey, FrameData>> GetLocalEntries()
        {
            foreach (var cacheName in m_frameCacheNames)
            {
                var frameCache = CacheFactory.GetOrCreateFrameCache(m_ignite, cacheName);
                foreach(var entry in frameCache.GetLocalEntries())
                {
                    yield return entry;
                }
            }
        }

        /// <summary>
        /// Returns an enumeration of frames that belong to the given flow. 
        /// To get the result the method uses an index-based  
        /// query over all caches specified in <see cref="m_frameCacheNames"/>.
        /// </summary>
        /// <param name="flowKey">The key of the flow.</param>
        /// <returns>An enumeration of frames that belong to the given flow.</returns>
        public IEnumerable<ICacheEntry<FrameKey, FrameData>> GetFrames(FlowKey flowKey)
        {
            var frameKeyProvider = new FrameKeyProvider();
            foreach (var compiledQuery in m_compiledQueries)
            {
                var queryResult = compiledQuery(flowKey.FlowKeyHash);
                foreach (var cacheEntry in queryResult)
                {
                    var frameKey = frameKeyProvider.GetKey(cacheEntry.Value);
                    if (FlowKey.Compare(flowKey, frameKey))
                    {
                        yield return cacheEntry;
                    }
                }
            }                                                                                                     
        }
        /// <summary>
        /// Returns an enumeration of frames that belong to the given flow. 
        /// To get the result the method performs a full scan 
        /// query over all caches specified in <see cref="m_frameCacheNames"/>.
        /// </summary>
        /// <param name="flowKey">The key of the flow.</param>
        /// <returns>An enumeration of frames that belong to the given flow.</returns>
        public IEnumerable<ICacheEntry<FrameKey, FrameData>> GetFramesByScan(FlowKey flowKey)
        {
            var scanQuery = new ScanQuery<FrameKey, FrameData>(new CacheEntryFrameFilter(flowKey));
            foreach (var cacheName in m_frameCacheNames)
            {
                var frameCache = CacheFactory.GetOrCreateFrameCache(m_ignite, cacheName);                
                var queryCursor = frameCache.Query(scanQuery);
                foreach (var cacheEntry in queryCursor)
                {
                    yield return cacheEntry;
                }
            }
        }
    }
}
