using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Kaitai;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public class DnsAnalyzer : IComputeAction
    {    
        public static IEnumerable<Model.DnsObject> Inspect(FlowKey flowKey, PacketStream flowRecord)
        {
            var sourceEndpoint = new IPEndPoint(new System.Net.IPAddress(flowKey.SourceAddress.ToArray()), flowKey.SourcePort);
            var destinationEndpoint = new IPEndPoint(new System.Net.IPAddress(flowKey.DestinationAddress.ToArray()), flowKey.DestinationPort);

            var flowId = PacketFlow.NewUid(flowKey.Protocol.ToString(), sourceEndpoint, destinationEndpoint, flowRecord.FirstSeen);

            // DNS response?
            if (flowKey.Protocol == ProtocolType.Udp && flowKey.SourcePort == 53)
            {
                var dns = InspectPackets(destinationEndpoint, sourceEndpoint, flowId, flowRecord.FrameList);
                return dns;
            }

            // DNS request?
            if (flowKey.Protocol == ProtocolType.Udp && destinationEndpoint.Port == 53)
            {
                var dns = InspectPackets(sourceEndpoint, destinationEndpoint, flowId, flowRecord.FrameList);
                return dns;
            }
            return Array.Empty<Model.DnsObject>();
        }


        private static IEnumerable<Model.DnsObject> InspectPackets(System.Net.IPEndPoint client, System.Net.IPEndPoint server, Guid flowId, IEnumerable<Frame> packetList)
        {
            var result = new List<DnsObject>(); 
            foreach (var frame in packetList)
            {
                try
                {
                    var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
                    var udpPacket = packet.Extract(typeof(UdpPacket)) as UdpPacket;
                    var stream = new KaitaiStream(udpPacket.PayloadData);
                    var dnsPacket = new Netdx.Packets.Core.DnsPacket(stream);
                    foreach (var (answer, index) in dnsPacket.Answers.Select((x, i) => (x, i + 1)))
                    {
                        var dnsModel = new Model.DnsObject
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
                        result.Add(dnsModel);
                    }
                }
                catch(Exception)
                {
                    // TODO: LOG Errors!
                }
            }
            return result;
        }


        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public void Invoke()
        {
            var flowCache = m_ignite.GetCache<FlowKey, PacketStream>(IgniteConfiguration.FlowCache);
            var dnsObjectCache = m_ignite.GetOrCreateCache<string, DnsObject> ("DnsObjectCache");

            foreach (var flow in flowCache.GetLocalEntries())
            {
                foreach (var dns in Inspect(flow.Key, flow.Value))
                {
                    dnsObjectCache.Put(dns.ObjectName, dns);
                }
            }            
        }
    }
}
