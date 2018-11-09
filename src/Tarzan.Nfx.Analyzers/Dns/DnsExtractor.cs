using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Apache.Ignite.Linq;
using Kaitai;
using NLog;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Core;

namespace Tarzan.Nfx.Analyzers
{
    public class DnsExtractor : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public string FlowCacheName { get; }

        public IEnumerable<string> FrameCacheNames { get; }

        public string DnsCacheName { get; }

        public DnsExtractor(string flowCacheName, IEnumerable<string> frameCacheNames, string dnsCacheName)
        {
            FlowCacheName = flowCacheName;
            FrameCacheNames = frameCacheNames;
            DnsCacheName = dnsCacheName;
        }

        public void Invoke()
        {
            m_ignite.Logger.Log(Apache.Ignite.Core.Log.LogLevel.Info, $"Extracting DNS objects from {FlowCacheName} started.", null, null, nameof(DnsExtractor), "", null);
            var flowCache = CacheFactory.GetOrCreateFlowCache(m_ignite, FlowCacheName).AsCacheQueryable(local: true);            
            var frameCache = CacheFactory.GetFrameCacheCollection(m_ignite, FrameCacheNames);
            var dnsObjectCache = CacheFactory.GetOrCreateCache<string, DnsObject>(m_ignite, DnsCacheName);

            var dnsFlows = flowCache.Where(f => f.Value.ServiceName == "domain");
            foreach (var dnsFlow in dnsFlows)
            {
                var frames = frameCache.GetItems(dnsFlow.Key);                
                var dnsObjects = Inspect(dnsFlow.Key, dnsFlow.Value, frames.Select(x => x.Value)).Select(x => KeyValuePair.Create(x.ObjectName, x)).ToList();
                dnsObjectCache.PutAll(dnsObjects);
            }
            m_ignite.Logger.Log(Apache.Ignite.Core.Log.LogLevel.Info, $"Extracting DNS objects from {FlowCacheName} completed.", null, null, nameof(DnsExtractor), "", null);
        }

        private IEnumerable<DnsObject> Inspect(FlowKey flowKey, FlowData flowData, IEnumerable<FrameData> frames)
        {
            var sourceEndpoint = flowKey.SourceEndpoint;
            var destinationEndpoint = flowKey.DestinationEndpoint;

            var flowId = flowData.FlowUid;

            // DNS response?
            if (flowKey.Protocol == ProtocolType.Udp && flowKey.SourcePort == 53)
            {
                var dns = ExtractDnsObjects(destinationEndpoint, sourceEndpoint, flowId, frames);
                return dns;
            }

            // DNS request?
            if (flowKey.Protocol == ProtocolType.Udp && flowKey.DestinationPort == 53)
            {
                var dns = ExtractDnsObjects(sourceEndpoint, destinationEndpoint, flowId, frames);
                return dns;
            }
            return Array.Empty<Model.DnsObject>();
        }


        private IEnumerable<Model.DnsObject> ExtractDnsObjects(System.Net.IPEndPoint client, System.Net.IPEndPoint server, string flowId, IEnumerable<FrameData> packetList)
        {
            var result = new List<DnsObject>(); 
            foreach (var frame in packetList)
            {
                try
                {
                    var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
                    var udpPacket = packet.Extract(typeof(UdpPacket)) as UdpPacket;
                    var stream = new KaitaiStream(udpPacket.PayloadData);
                    var dnsPacket = new DnsPacket(stream);
                    var clientString = client.ToString();
                    var serverString = server.ToString();
                    foreach (var (answer, index) in dnsPacket.Answers.Select((x, i) => (x, i + 1)))
                    {
                        var dnsObject = new Model.DnsObject
                        {
                            Client = clientString,
                            Server = serverString,
                            Timestamp = frame.Timestamp,
                            FlowUid = flowId,
                            TransactionId = $"{dnsPacket.TransactionId.ToString("X4")}-{index.ToString("D4")}",
                            DnsType = answer.Type.ToString().ToUpperInvariant(),
                            DnsTtl = answer.Ttl,
                            DnsQuery = answer.Name.DomainNameString,
                            DnsAnswer = answer.AnswerString,
                        };
                        result.Add(dnsObject);
                    }
                }
                catch(Exception e)
                {
                    m_ignite.Logger.Log(Apache.Ignite.Core.Log.LogLevel.Error, "InpsectPackets error", null, null, "Error", "", e);
                }
            }
            return result;
        }
    }
}
