using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Apache.Ignite.Linq;
using Kaitai;
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
    public class DnsAnalyzer : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public string FlowCacheName { get; }

        public IEnumerable<string> FrameCacheNames { get; }

        public string DnsCacheName { get; }

        public DnsAnalyzer(string flowCacheName, IEnumerable<string> frameCacheNames, string dnsCacheName)
        {
            FlowCacheName = flowCacheName;
            FrameCacheNames = frameCacheNames;
            DnsCacheName = dnsCacheName;
        }

        public void Invoke()
        {
            Console.WriteLine($"Analyzing {FlowCacheName}...");
            var flowCache = CacheFactory.GetOrCreateFlowCache(m_ignite, FlowCacheName).AsCacheQueryable(local: true);            
            var frameCache = new FrameCacheCollection(m_ignite, FrameCacheNames);
            var dnsObjectCache = CacheFactory.GetOrCreateCache<string, DnsObject>(m_ignite, DnsCacheName);

            var dnsFlows = flowCache.Where(f => f.Value.ServiceName == "domain");
            foreach (var dnsFlow in dnsFlows)
            {
                Console.Write($"Found DNS flow {dnsFlow.Key.ToString()}, getting frames...");
                var frames = frameCache.GetFrames(dnsFlow.Key);
                Console.Write($"inspecting frames...");
                var dnsObjects = Inspect(dnsFlow.Key, dnsFlow.Value, frames.Select(x => x.Value)).Select(x => KeyValuePair.Create(x.ObjectName, x)).ToList();
                Console.Write($"done, found {dnsObjects.Count} items, storing to {DnsCacheName}...");
                dnsObjectCache.PutAll(dnsObjects);
                Console.WriteLine("ok.");
            }
        }

        private static IEnumerable<DnsObject> Inspect(FlowKey flowKey, FlowData flowData, IEnumerable<FrameData> frames)
        {
            var sourceEndpoint = flowKey.SourceEndpoint;
            var destinationEndpoint = flowKey.DestinationEndpoint;

            var flowId = flowData.FlowUid;

            // DNS response?
            if (flowKey.Protocol == ProtocolType.Udp && flowKey.SourcePort == 53)
            {
                var dns = InspectPackets(destinationEndpoint, sourceEndpoint, flowId, frames);
                return dns;
            }

            // DNS request?
            if (flowKey.Protocol == ProtocolType.Udp && flowKey.DestinationPort == 53)
            {
                var dns = InspectPackets(sourceEndpoint, destinationEndpoint, flowId, frames);
                return dns;
            }
            return Array.Empty<Model.DnsObject>();
        }


        private static IEnumerable<Model.DnsObject> InspectPackets(System.Net.IPEndPoint client, System.Net.IPEndPoint server, string flowId, IEnumerable<FrameData> packetList)
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
                    foreach (var (answer, index) in dnsPacket.Answers.Select((x, i) => (x, i + 1)))
                    {
                        var dnsObject = new Model.DnsObject
                        {
                            Client = client.ToString(),
                            Server = server.ToString(),
                            Timestamp = frame.Timestamp,
                            FlowUid = flowId.ToString(),
                            TransactionId = $"{dnsPacket.TransactionId.ToString("X4")}-{index.ToString("D4")}",
                            DnsType = answer.Type.ToString().ToUpperInvariant(),
                            DnsTtl = answer.Ttl,
                            DnsQuery = answer.Name.DomainNameString,
                            DnsAnswer = answer.AnswerString,
                        };
                        result.Add(dnsObject);
                    }
                }
                catch(Exception)
                {
                    // TODO: LOG Errors!
                }
            }
            return result;
        }
    }
}
