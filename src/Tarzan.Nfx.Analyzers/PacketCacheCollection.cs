using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers
{
    public static class PacketCacheCollection 
    {
        public static IEnumerable<FrameData> GetOrderedPackets(this IReferencedCache<FlowKey,FrameKey,FrameData> frameCacheCollection, FlowKey flowKey)
        {
            return frameCacheCollection.GetItems(flowKey).OrderBy(f=>f.Value.Timestamp).Select(f=>f.Value);
        }
        public static Conversation<IEnumerable<FrameData>> GetConversation(this IReferencedCache<FlowKey,FrameKey,FrameData> frameCache, FlowKey flowKey, bool detectClientFromTimestamps = false)
        {
            var oppositeFlowKey = flowKey.SwapEndpoints();
            var upflowPackets = frameCache.GetOrderedPackets(flowKey);

            var downflowPackets = frameCache.GetOrderedPackets(oppositeFlowKey);
            if (detectClientFromTimestamps)
            {
                var upTs = upflowPackets.FirstOrDefault()?.Timestamp;
                var downTs = downflowPackets.FirstOrDefault()?.Timestamp;
                if (upTs < downTs)
                {
                    return new Conversation<IEnumerable<FrameData>>(flowKey, upflowPackets, downflowPackets);
                }
                else
                {
                    return new Conversation<IEnumerable<FrameData>>(oppositeFlowKey, downflowPackets, upflowPackets);
                }
            }
            else
            {
                return new Conversation<IEnumerable<FrameData>>(flowKey, upflowPackets, downflowPackets);
            }
        }
        /// <summary>
        /// Merges two ordered sequence producing a single still ordered sequence. 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<T> MergeOrdered<T>(Func<T, T, bool> lessOrEqual, IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            var enumeratorFirst = first.GetEnumerator();
            var enumeratorSecond = second.GetEnumerator();
            var firstHasItems = enumeratorFirst.MoveNext();
            var secondHasItems = enumeratorSecond.MoveNext();
            while (firstHasItems && secondHasItems)
            {
                if (lessOrEqual(enumeratorFirst.Current, enumeratorSecond.Current))
                {
                    yield return enumeratorFirst.Current;
                    firstHasItems = enumeratorFirst.MoveNext();
                }
                else
                {
                    yield return enumeratorSecond.Current;
                    secondHasItems = enumeratorSecond.MoveNext();
                }
            }
            if (secondHasItems)
            {
                do
                {
                    yield return enumeratorSecond.Current;
                } while (enumeratorSecond.MoveNext());
            }
            if (firstHasItems)
            {
                do
                {
                    yield return enumeratorFirst.Current;
                } while (enumeratorFirst.MoveNext());
            }
        }
    }
}
