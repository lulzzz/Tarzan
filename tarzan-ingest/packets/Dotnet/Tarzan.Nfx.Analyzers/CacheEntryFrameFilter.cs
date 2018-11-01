using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;

namespace Tarzan.Nfx.Analyzers
{
    /// <summary>
    /// It provides frames for the spcified flow key from the given collection of frame caches.
    /// </summary>
    public class FrameCacheFlowProvider
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
        /// <param name="ignite">An active instance of the ignite service.</param>
        /// <param name="frameCacheNames">Collection of names of frame caches.</param>
        public FrameCacheFlowProvider(IIgnite ignite, IEnumerable<string> frameCacheNames)
        {
            
            m_ignite = ignite;
            m_frameCacheNames = frameCacheNames.ToArray();
            m_compiledQueries = new Func<int, IQueryCursor<ICacheEntry<FrameKey, FrameData>>>[m_frameCacheNames.Length];
            for(int i = 0; i < m_frameCacheNames.Length; i++)
            {
                var queryable = CacheFactory.GetOrCreateFrameCache(m_ignite, m_frameCacheNames[i]).AsCacheQueryable(local: true);
                m_compiledQueries[i] = CompiledQuery.Compile((int hashCode) => queryable.Where(x => x.Key.FlowKeyHash == hashCode));
            }
        }

        /// <summary>
        /// Returns an enumeration of frames that belong to the given flow. 
        /// To get the result the method uses an index-based  
        /// query over all caches specified in <see cref="m_frameCacheNames"/>.
        /// </summary>
        /// <param name="flowKey">The key of the flow.</param>
        /// <returns>An enumeration of frames that belong to the given flow.</returns>
        public IEnumerable<KeyValuePair<FrameKey, FrameData>> GetFrames(FlowKey flowKey)
        {
            var frameKeyProvider = new FrameKeyProvider();
            foreach (var compiledQuery in m_compiledQueries)
            {
                var queryResult = compiledQuery(flowKey.HashCode);
                foreach (var cacheEntry in queryResult)
                {
                    var frameKey = frameKeyProvider.GetKey(cacheEntry.Value);
                    if (FlowKey.Compare(flowKey, frameKey))
                    {
                        yield return KeyValuePair.Create(cacheEntry.Key, cacheEntry.Value);
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
        public IEnumerable<KeyValuePair<FrameKey, FrameData>> GetFramesByScan(FlowKey flowKey)
        {
            var scanQuery = new ScanQuery<FrameKey, FrameData>(new CacheEntryFrameFilter(flowKey));
            foreach (var cacheName in m_frameCacheNames)
            {
                var frameCache = CacheFactory.GetOrCreateFrameCache(m_ignite, cacheName);
                var queryCursor = frameCache.Query(scanQuery);
                foreach (var cacheEntry in queryCursor)
                {
                    yield return KeyValuePair.Create(cacheEntry.Key, cacheEntry.Value);
                }
            }
        }
    }

    /// <summary>
    /// This filter selects all frames of the specified flow.
    /// </summary>  
    [Serializable]
    public class CacheEntryFrameFilter : ICacheEntryFilter<FrameKey, FrameData>
    {
        public FlowKey FlowKey { get; set; }
        public IKeyProvider<FlowKey, FrameData> KeyProvider { get; set; }

        public CacheEntryFrameFilter(FlowKey flowKey)
        {
            FlowKey = flowKey;
            KeyProvider = new FrameKeyProvider();
        }

        public CacheEntryFrameFilter()
        {
        }

        public bool Invoke(ICacheEntry<FrameKey, FrameData> frame)
        {
            return FlowKey.HashCode != frame.Key.FlowKeyHash ? false : FlowKey.Equals(KeyProvider.GetKey(frame.Value));
        }
    }
}
