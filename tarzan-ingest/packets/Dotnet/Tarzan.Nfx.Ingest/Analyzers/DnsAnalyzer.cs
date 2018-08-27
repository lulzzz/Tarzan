using Kaitai;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public static class DnsAnalyzer
    {    
        public static IEnumerable<Model.DnsObject> Inspect(PacketFlowKey flowKey, PacketStream flowRecord)
        {
            var sourceEndpoint = new IPEndPoint(new System.Net.IPAddress(flowKey.SourceAddress.ToArray()), flowKey.SourcePort);
            var destinationEndpoint = new IPEndPoint(new System.Net.IPAddress(flowKey.DestinationAddress.ToArray()), flowKey.DestinationPort);

            var flowId = PacketFlow.NewUid(flowKey.Protocol.ToString(), sourceEndpoint, destinationEndpoint, flowRecord.FirstSeen);

            // DNS response?
            if (flowKey.Protocol == ProtocolType.Udp && flowKey.SourcePort == 53)
            {
                var dns = InspectPackets(destinationEndpoint, sourceEndpoint, flowId, flowRecord.PacketList);
                return dns;
            }

            // DNS request?
            if (flowKey.Protocol == ProtocolType.Udp && destinationEndpoint.Port == 53)
            {
                var dns = InspectPackets(sourceEndpoint, destinationEndpoint, flowId, flowRecord.PacketList);
                return dns;
            }
            return Array.Empty<Model.DnsObject>();
        }


        private static IEnumerable<Model.DnsObject> InspectPackets(System.Net.IPEndPoint client, System.Net.IPEndPoint server, Guid flowId, IEnumerable<(Packet packet, PosixTimeval time)> packetList)
        {
            var result = new List<DnsObject>(); 
            foreach ((var packet, var time) in packetList)
            {
                try
                {
                    var udpPacket = packet.Extract(typeof(UdpPacket)) as UdpPacket;
                    var stream = new KaitaiStream(udpPacket.PayloadData);
                    var dnsPacket = new Netdx.Packets.Core.DnsPacket(stream);
                    foreach (var (answer, index) in dnsPacket.Answers.Select((x, i) => (x, i + 1)))
                    {
                        var dnsModel = new Model.DnsObject
                        {
                            Client = client.ToString(),
                            Server = server.ToString(),
                            Timestamp = time.ToUnixTimeMilliseconds(),
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
    }
}
