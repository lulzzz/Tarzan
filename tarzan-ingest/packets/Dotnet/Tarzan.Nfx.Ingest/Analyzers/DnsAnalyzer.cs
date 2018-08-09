using Kaitai;
using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public static class DnsAnalyzer
    {    
        public static IEnumerable<Model.Dns> Inspect(FlowKey flowKey, FlowRecordWithPackets flowRecord)
        {
            // DNS response?
            if (flowKey.Protocol == ProtocolType.UDP && flowKey.SourceEndpoint.Port == 53)
            {
                var dns = InspectPackets(flowKey.DestinationEndpoint, flowKey.SourceEndpoint, flowRecord.FlowId, flowRecord.PacketList);
                return dns;
            }

            // DNS request?
            if (flowKey.Protocol == ProtocolType.UDP && flowKey.DestinationEndpoint.Port == 53)
            {
                var dns = InspectPackets(flowKey.SourceEndpoint, flowKey.DestinationEndpoint, flowRecord.FlowId, flowRecord.PacketList);
                return dns;
            }
            return Array.Empty<Model.Dns>();
        }


        private static IEnumerable<Model.Dns> InspectPackets(System.Net.IPEndPoint client, System.Net.IPEndPoint server, Guid flowId, IEnumerable<(Packet packet, PosixTimeval time)> packetList)
        {
            foreach ((var packet, var time) in packetList)
            {
                var udpPacket = packet.Extract(typeof(UdpPacket)) as UdpPacket;
                var stream = new KaitaiStream(udpPacket.PayloadData);
                var dnsPacket = new Netdx.Packets.Core.DnsPacket(stream);
                foreach (var (answer, index) in dnsPacket.Answers.Select((x,i) => (x,i+1)))
                {
                    var dnsModel = new Model.Dns
                    {
                        Client = client.ToString(),
                        Server = server.ToString(),
                        Timestamp = (long)time.MicroSeconds,
                        FlowId = flowId.ToString(), 
                        DnsId = $"{dnsPacket.TransactionId}-{index.ToString("D2")}",
                        DnsType = answer.Type.ToString().ToUpperInvariant(),
                        DnsTtl = answer.Ttl,
                        DnsQuery = answer.Name.DomainNameString,
                        DnsAnswer = answer.AnswerString,
                    };
                    yield return dnsModel;
                }
            }
        }
    }
}
