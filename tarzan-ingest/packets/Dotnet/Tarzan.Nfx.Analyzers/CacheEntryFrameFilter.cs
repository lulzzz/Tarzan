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
        /// Creates a new frame provider that searches for frames in the specified collection of caches.
        /// </summary>
        /// <param name="ignite">An active instance of the ignite service.</param>
        /// <param name="frameCacheNames">Collection of names of frame caches.</param>
        public FrameCacheFlowProvider(IIgnite ignite, IEnumerable<string> frameCacheNames)
        {
            FrameCacheNames = frameCacheNames.ToArray();
            m_ignite = ignite;
        }

        /// <summary>
        /// Names of frame caches.
        /// </summary>
        public string[] FrameCacheNames { get; }

        /// <summary>
        /// Returns an enumeration of frames that belong to the given flow. 
        /// To get the result the method performs a full scan 
        /// query over all caches specified in <see cref="FrameCacheNames"/>.
        /// </summary>
        /// <param name="flowKey">The key of the flow.</param>
        /// <returns>An enumeration of frames that belong to the given flow.</returns>
        public IEnumerable<KeyValuePair<FrameKey, FrameData>> GetFrames(FlowKey flowKey)
        {
            foreach (var cacheName in FrameCacheNames)
            {
                var frameCache = CacheFactory.GetOrCreateFrameCache(m_ignite, cacheName).AsCacheQueryable();
                var queryResult = frameCache.Where(x => x.Key.FlowKeyHash == flowKey.HashCode);
                foreach (var cacheEntry in queryResult)
                {
                    yield return KeyValuePair.Create(cacheEntry.Key, cacheEntry.Value);
                }
            }                                                                                                     
        }

        public IEnumerable<KeyValuePair<FrameKey, FrameData>> GetFramesByScan(FlowKey flowKey)
        {
            var scanQuery = new ScanQuery<FrameKey, FrameData>(new CacheEntryFrameFilter(flowKey));
            foreach (var cacheName in FrameCacheNames)
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
