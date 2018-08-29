using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Expiry;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Resource;
using Netdx.PacketDecoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class FlowCache : ICache<PacketFlowKey, PacketStream>
    {
        readonly ICacheEntryProcessor<PacketFlowKey, PacketStream, PacketStream, PacketStream> m_cacheEntryProcessor;

        ICache<PacketFlowKey, PacketStream> m_cache;

        public FlowCache(IIgnite ignite)
        {
            m_cache = ignite.GetOrCreateCache<PacketFlowKey, PacketStream>(nameof(FlowCache));
            m_cacheEntryProcessor = new MergePacketStream();
        }

        public PacketStream this[PacketFlowKey key] { get => m_cache[key]; set => m_cache[key] = value; }

        public static CacheConfiguration DefaultConfiguration =>
                                new CacheConfiguration(nameof(FlowCache), new QueryEntity(typeof(PacketStream)))
                                {
                                    CacheMode = CacheMode.Partitioned,
                                    Backups = 0
                                };

        public IQueryable<ICacheEntry<PacketFlowKey, PacketStream>> AsCacheQueryable() => m_cache.AsQueryable();

        public string Name => m_cache.Name;

        public IIgnite Ignite => m_cache.Ignite;

        public bool IsKeepBinary => m_cache.IsKeepBinary;

        public void Clear()
        {
            m_cache.Clear();
        }

        public void Clear(PacketFlowKey key)
        {
            m_cache.Clear(key);
        }

        public void ClearAll(IEnumerable<PacketFlowKey> keys)
        {
            m_cache.ClearAll(keys);
        }

        public Task ClearAllAsync(IEnumerable<PacketFlowKey> keys)
        {
            return m_cache.ClearAllAsync(keys);
        }

        public Task ClearAsync()
        {
            return m_cache.ClearAsync();
        }

        public Task ClearAsync(PacketFlowKey key)
        {
            return m_cache.ClearAsync(key);
        }

        public bool ContainsKey(PacketFlowKey key)
        {
            return m_cache.ContainsKey(key);
        }

        public Task<bool> ContainsKeyAsync(PacketFlowKey key)
        {
            return m_cache.ContainsKeyAsync(key);
        }

        public bool ContainsKeys(IEnumerable<PacketFlowKey> keys)
        {
            return m_cache.ContainsKeys(keys);
        }

        public Task<bool> ContainsKeysAsync(IEnumerable<PacketFlowKey> keys)
        {
            return m_cache.ContainsKeysAsync(keys);
        }

        public PacketStream Get(PacketFlowKey key)
        {
            return m_cache.Get(key);
        }

        public ICollection<ICacheEntry<PacketFlowKey, PacketStream>> GetAll(IEnumerable<PacketFlowKey> keys)
        {
            return m_cache.GetAll(keys);
        }

        public Task<ICollection<ICacheEntry<PacketFlowKey, PacketStream>>> GetAllAsync(IEnumerable<PacketFlowKey> keys)
        {
            return m_cache.GetAllAsync(keys);
        }

        public CacheResult<PacketStream> GetAndPut(PacketFlowKey key, PacketStream val)
        {
            return m_cache.GetAndPut(key, val);
        }

        public Task<CacheResult<PacketStream>> GetAndPutAsync(PacketFlowKey key, PacketStream val)
        {
            return m_cache.GetAndPutAsync(key, val);
        }

        public CacheResult<PacketStream> GetAndPutIfAbsent(PacketFlowKey key, PacketStream val)
        {
            return m_cache.GetAndPutIfAbsent(key, val);
        }

        public Task<CacheResult<PacketStream>> GetAndPutIfAbsentAsync(PacketFlowKey key, PacketStream val)
        {
            return m_cache.GetAndPutIfAbsentAsync(key, val);
        }

        public CacheResult<PacketStream> GetAndRemove(PacketFlowKey key)
        {
            return m_cache.GetAndRemove(key);
        }

        public Task<CacheResult<PacketStream>> GetAndRemoveAsync(PacketFlowKey key)
        {
            return m_cache.GetAndRemoveAsync(key);
        }

        public CacheResult<PacketStream> GetAndReplace(PacketFlowKey key, PacketStream val)
        {
            return m_cache.GetAndReplace(key, val);
        }

        public Task<CacheResult<PacketStream>> GetAndReplaceAsync(PacketFlowKey key, PacketStream val)
        {
            return m_cache.GetAndReplaceAsync(key, val);
        }

        public Task<PacketStream> GetAsync(PacketFlowKey key)
        {
            return m_cache.GetAsync(key);
        }

        public CacheConfiguration GetConfiguration()
        {
            return m_cache.GetConfiguration();
        }

        public IEnumerator<ICacheEntry<PacketFlowKey, PacketStream>> GetEnumerator()
        {
            return m_cache.GetEnumerator();
        }

        public IEnumerable<ICacheEntry<PacketFlowKey, PacketStream>> GetLocalEntries(params CachePeekMode[] peekModes)
        {
            return m_cache.GetLocalEntries(peekModes);
        }

        public ICacheMetrics GetLocalMetrics()
        {
            return m_cache.GetLocalMetrics();
        }

        public int GetLocalSize(params CachePeekMode[] modes)
        {
            return m_cache.GetLocalSize(modes);
        }

        public ICollection<int> GetLostPartitions()
        {
            return m_cache.GetLostPartitions();
        }

        public ICacheMetrics GetMetrics()
        {
            return m_cache.GetMetrics();
        }

        public ICacheMetrics GetMetrics(IClusterGroup clusterGroup)
        {
            return m_cache.GetMetrics(clusterGroup);
        }

        public int GetSize(params CachePeekMode[] modes)
        {
            return m_cache.GetSize(modes);
        }

        public Task<int> GetSizeAsync(params CachePeekMode[] modes)
        {
            return m_cache.GetSizeAsync(modes);
        }

        public TRes Invoke<TArg, TRes>(PacketFlowKey key, ICacheEntryProcessor<PacketFlowKey, PacketStream, TArg, TRes> processor, TArg arg)
        {
            return m_cache.Invoke(key, processor, arg);
        }

        public ICollection<ICacheEntryProcessorResult<PacketFlowKey, TRes>> InvokeAll<TArg, TRes>(IEnumerable<PacketFlowKey> keys, ICacheEntryProcessor<PacketFlowKey, PacketStream, TArg, TRes> processor, TArg arg)
        {
            return m_cache.InvokeAll(keys, processor, arg);
        }

        public Task<ICollection<ICacheEntryProcessorResult<PacketFlowKey, TRes>>> InvokeAllAsync<TArg, TRes>(IEnumerable<PacketFlowKey> keys, ICacheEntryProcessor<PacketFlowKey, PacketStream, TArg, TRes> processor, TArg arg)
        {
            return m_cache.InvokeAllAsync(keys, processor, arg);
        }

        public Task<TRes> InvokeAsync<TArg, TRes>(PacketFlowKey key, ICacheEntryProcessor<PacketFlowKey, PacketStream, TArg, TRes> processor, TArg arg)
        {
            return m_cache.InvokeAsync(key, processor, arg);
        }

        public bool IsEmpty()
        {
            return m_cache.IsEmpty();
        }

        public bool IsLocalLocked(PacketFlowKey key, bool byCurrentThread)
        {
            return m_cache.IsLocalLocked(key, byCurrentThread);
        }

        public void LoadAll(IEnumerable<PacketFlowKey> keys, bool replaceExistingValues)
        {
            m_cache.LoadAll(keys, replaceExistingValues);
        }

        public Task LoadAllAsync(IEnumerable<PacketFlowKey> keys, bool replaceExistingValues)
        {
            return m_cache.LoadAllAsync(keys, replaceExistingValues);
        }

        public void LoadCache(ICacheEntryFilter<PacketFlowKey, PacketStream> p, params object[] args)
        {
            m_cache.LoadCache(p, args);
        }

        public Task LoadCacheAsync(ICacheEntryFilter<PacketFlowKey, PacketStream> p, params object[] args)
        {
            return m_cache.LoadCacheAsync(p, args);
        }

        public void LocalClear(PacketFlowKey key)
        {
            m_cache.LocalClear(key);
        }

        public void LocalClearAll(IEnumerable<PacketFlowKey> keys)
        {
            m_cache.LocalClearAll(keys);
        }

        public void LocalEvict(IEnumerable<PacketFlowKey> keys)
        {
            m_cache.LocalEvict(keys);
        }

        public void LocalLoadCache(ICacheEntryFilter<PacketFlowKey, PacketStream> p, params object[] args)
        {
            m_cache.LocalLoadCache(p, args);
        }

        public Task LocalLoadCacheAsync(ICacheEntryFilter<PacketFlowKey, PacketStream> p, params object[] args)
        {
            return m_cache.LocalLoadCacheAsync(p, args);
        }

        public PacketStream LocalPeek(PacketFlowKey key, params CachePeekMode[] modes)
        {
            return m_cache.LocalPeek(key, modes);
        }

        public ICacheLock Lock(PacketFlowKey key)
        {
            return m_cache.Lock(key);
        }

        public ICacheLock LockAll(IEnumerable<PacketFlowKey> keys)
        {
            return m_cache.LockAll(keys);
        }

        public void Put(PacketFlowKey key, PacketStream val)
        {
            m_cache.Put(key, val);
        }

        public void PutAll(IEnumerable<KeyValuePair<PacketFlowKey, PacketStream>> vals)
        {
            m_cache.PutAll(vals);
        }

        public Task PutAllAsync(IEnumerable<KeyValuePair<PacketFlowKey, PacketStream>> vals)
        {
            return m_cache.PutAllAsync(vals);
        }

        public Task PutAsync(PacketFlowKey key, PacketStream val)
        {
            return m_cache.PutAsync(key, val);
        }

        public bool PutIfAbsent(PacketFlowKey key, PacketStream val)
        {
            return m_cache.PutIfAbsent(key, val);
        }

        public Task<bool> PutIfAbsentAsync(PacketFlowKey key, PacketStream val)
        {
            return m_cache.PutIfAbsentAsync(key, val);
        }

        public IQueryCursor<ICacheEntry<PacketFlowKey, PacketStream>> Query(QueryBase qry)
        {
            return m_cache.Query(qry);
        }

        public IFieldsQueryCursor Query(SqlFieldsQuery qry)
        {
            return m_cache.Query(qry);
        }

        public IContinuousQueryHandle QueryContinuous(ContinuousQuery<PacketFlowKey, PacketStream> qry)
        {
            return m_cache.QueryContinuous(qry);
        }

        public IContinuousQueryHandle<ICacheEntry<PacketFlowKey, PacketStream>> QueryContinuous(ContinuousQuery<PacketFlowKey, PacketStream> qry, QueryBase initialQry)
        {
            return m_cache.QueryContinuous(qry, initialQry);
        }

        public IQueryCursor<IList> QueryFields(SqlFieldsQuery qry)
        {
            return m_cache.QueryFields(qry);
        }

        public Task Rebalance()
        {
            return m_cache.Rebalance();
        }

        public bool Remove(PacketFlowKey key)
        {
            return m_cache.Remove(key);
        }

        public bool Remove(PacketFlowKey key, PacketStream val)
        {
            return m_cache.Remove(key, val);
        }

        public void RemoveAll(IEnumerable<PacketFlowKey> keys)
        {
            m_cache.RemoveAll(keys);
        }

        public void RemoveAll()
        {
            m_cache.RemoveAll();
        }

        public Task RemoveAllAsync(IEnumerable<PacketFlowKey> keys)
        {
            return m_cache.RemoveAllAsync(keys);
        }

        public Task RemoveAllAsync()
        {
            return m_cache.RemoveAllAsync();
        }

        public Task<bool> RemoveAsync(PacketFlowKey key)
        {
            return m_cache.RemoveAsync(key);
        }

        public Task<bool> RemoveAsync(PacketFlowKey key, PacketStream val)
        {
            return m_cache.RemoveAsync(key, val);
        }

        public bool Replace(PacketFlowKey key, PacketStream val)
        {
            return m_cache.Replace(key, val);
        }

        public bool Replace(PacketFlowKey key, PacketStream oldVal, PacketStream newVal)
        {
            return m_cache.Replace(key, oldVal, newVal);
        }

        public Task<bool> ReplaceAsync(PacketFlowKey key, PacketStream val)
        {
            return m_cache.ReplaceAsync(key, val);
        }

        public Task<bool> ReplaceAsync(PacketFlowKey key, PacketStream oldVal, PacketStream newVal)
        {
            return m_cache.ReplaceAsync(key, oldVal, newVal);
        }

        public bool TryGet(PacketFlowKey key, out PacketStream value)
        {
            return m_cache.TryGet(key, out value);
        }

        public Task<CacheResult<PacketStream>> TryGetAsync(PacketFlowKey key)
        {
            return m_cache.TryGetAsync(key);
        }

        public bool TryLocalPeek(PacketFlowKey key, out PacketStream value, params CachePeekMode[] modes)
        {
            return m_cache.TryLocalPeek(key, out value, modes);
        }

        public PacketStream UpdateStream(PacketFlowKey key, PacketStream value)
        {
            return m_cache.Invoke(key, m_cacheEntryProcessor, value);
        }

        public ICache<PacketFlowKey, PacketStream> WithExpiryPolicy(IExpiryPolicy plc)
        {
            return m_cache.WithExpiryPolicy(plc);
        }

        public ICache<TK1, TV1> WithKeepBinary<TK1, TV1>()
        {
            return m_cache.WithKeepBinary<TK1, TV1>();
        }

        public ICache<PacketFlowKey, PacketStream> WithNoRetries()
        {
            return m_cache.WithNoRetries();
        }

        public ICache<PacketFlowKey, PacketStream> WithPartitionRecover()
        {
            return m_cache.WithPartitionRecover();
        }

        public ICache<PacketFlowKey, PacketStream> WithSkipStore()
        {
            return m_cache.WithSkipStore();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_cache.GetEnumerator();
        }

        public class MergePacketStream : ICacheEntryProcessor<PacketFlowKey, PacketStream, PacketStream, PacketStream>
        {   
            public PacketStream Process(IMutableCacheEntry<PacketFlowKey, PacketStream> entry, PacketStream arg)
            {
                entry.Value = PacketStream.Merge(entry.Value, arg);
                return entry.Value;
            }
        }
    }
}
