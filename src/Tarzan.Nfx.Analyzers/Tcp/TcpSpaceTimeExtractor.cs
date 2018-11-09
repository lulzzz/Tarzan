using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Tcp
{
    /// <summary>
    /// Extracts Space-Time information from all TCP flows located in 
    /// <see cref="FlowCacheName"/>.
    /// </summary>
    class TcpSpaceTimeExtractor : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;


        public TcpSpaceTimeExtractor(string flowCacheName, List<string> packetCacheNames, string tcpOutCacheName)
        {
            FlowCacheName = flowCacheName;
            FrameCacheNames = packetCacheNames;
            TcpSpaceTimeCacheName = tcpOutCacheName;
        }

        public string FlowCacheName { get; }

        public IEnumerable<string> FrameCacheNames { get; }

        public string TcpSpaceTimeCacheName { get; }

        public void Invoke()
        {
            var tcpCache = CacheFactory.GetOrCreateCache<FlowKey, TcpSpaceTimeModel>(m_ignite, TcpSpaceTimeCacheName);
            var flowCache = CacheFactory.GetOrCreateFlowCache(m_ignite, FlowCacheName);
            var frameCache = CacheFactory.GetFrameCacheCollection(m_ignite, FrameCacheNames);

            var tcpObjects = flowCache.GetLocalEntries()
                .Where(flowEntry => flowEntry.Key.Protocol == System.Net.Sockets.ProtocolType.Tcp && flowEntry.Key.SourcePort > flowEntry.Key.DestinationPort)
                .Select(flowEntry => ExtractTcpSpaceTimeModel(frameCache.GetConversation(flowEntry.Key, true), flowEntry))
                .Select(tcpModel => KeyValuePair.Create(tcpModel.FlowKey, tcpModel));
            tcpCache.PutAll(tcpObjects);
        }

        private TcpSpaceTimeModel ExtractTcpSpaceTimeModel(Conversation<IEnumerable<FrameData>> conversation, ICacheEntry<FlowKey, FlowData> flowEntry)
        {
            var originTimestamp = conversation.Upflow.FirstOrDefault()?.Timestamp ?? conversation.Downflow.FirstOrDefault()?.Timestamp ?? 0;
            var upflow = conversation.Upflow.Select(frameData => GetTcpPacketVector(originTimestamp, frameData, 1)).Where(x=> x!= null);
            var downflow = conversation.Downflow.Select(frameData => GetTcpPacketVector(originTimestamp, frameData, -1)).Where(x => x != null);
            var vectors = PacketCacheCollection.MergeOrdered((f1, f2) => f1.Offset <= f2.Offset, upflow, downflow);
            return new TcpSpaceTimeModel
            {
                FlowKey = conversation.ConversationKey,
                TimeOrigin = originTimestamp,
                Events = vectors.ToArray()
            };
        }
        private TcpSpaceTimeModel.Event GetTcpPacketVector(long originTimestamp, FrameData frameData, int sizeMultiplier)
        {
            if (Packet.ParsePacket((LinkLayers)frameData.LinkLayer, frameData.Data).Extract(typeof(TcpPacket)) is TcpPacket tcpPacket)
            {
                return new TcpSpaceTimeModel.Event
                {
                    Size = tcpPacket.PayloadData.Length * sizeMultiplier,
                    Offset = frameData.Timestamp - originTimestamp,
                    TcpFlags = tcpPacket.AllFlags
                };
            }
            return null;
        }
    }
}
