using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Kaitai;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public class DnsAnalyzer : IComputeAction
    {    
        public static IEnumerable<Model.DnsObject> Inspect(Tarzan.Nfx.FlowTracker.FlowKey flowKey, PacketStream packetStream)
        {
            var sourceEndpoint = new IPEndPoint(flowKey.SourceIpAddress, flowKey.SourcePort);
            var destinationEndpoint = new IPEndPoint(flowKey.DestinationIpAddress, flowKey.DestinationPort);

            var flowId = packetStream.FlowUid;

            // DNS response?
            if (flowKey.Protocol == ProtocolType.Udp && flowKey.SourcePort == 53)
            {
                var dns = InspectPackets(destinationEndpoint, sourceEndpoint, flowId, packetStream.FrameList);
                return dns;
            }

            // DNS request?
            if (flowKey.Protocol == ProtocolType.Udp && destinationEndpoint.Port == 53)
            {
                var dns = InspectPackets(sourceEndpoint, destinationEndpoint, flowId, packetStream.FrameList);
                return dns;
            }
            return Array.Empty<Model.DnsObject>();
        }


        private static IEnumerable<Model.DnsObject> InspectPackets(System.Net.IPEndPoint client, System.Net.IPEndPoint server, string flowId, IEnumerable<Tarzan.Nfx.FlowTracker.Frame> packetList)
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
            var flowCache = new PacketFlowTable(m_ignite).GetCache();
            var packetCache = new PacketStreamTable(m_ignite).GetCache();
            var dnsObjectCache = new DnsObjectTable(m_ignite);

            foreach(var dnsObject in flowCache.GetLocalEntries()
                .Where(f => String.Equals("domain", f.Value.ServiceName, StringComparison.InvariantCultureIgnoreCase)))
            {
                var packetStream = packetCache.Get(dnsObject.Value.FlowUid);
                var dnsObjects = Inspect(dnsObject.Key, packetStream).Select(x => KeyValuePair.Create(x.ObjectName, x));
                dnsObjectCache.GetOrCreateCache().PutAll(dnsObjects);
            }
        }
    }
}
