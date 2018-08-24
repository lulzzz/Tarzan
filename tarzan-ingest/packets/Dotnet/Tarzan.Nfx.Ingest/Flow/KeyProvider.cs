using System;
using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;

namespace Tarzan.Nfx.Ingest
{
        class KeyProvider : IKeyProvider<FlowKey, (Packet, PosixTimeval)>
        {
            public FlowKey GetKey((Packet, PosixTimeval) capture)
            {
                var packet = capture.Item1;
                FlowKey GetUdpFlowKey(UdpPacket udp)
                {
                    return new FlowKey()
                    {
                        Protocol = ProtocolType.UDP,
                        SourceEndpoint = new IPEndPoint((udp.ParentPacket as IpPacket).SourceAddress, udp.SourcePort),
                        DestinationEndpoint = new IPEndPoint((udp.ParentPacket as IpPacket).DestinationAddress, udp.DestinationPort),
                    };
                }
                FlowKey GetTcpFlowKey(TcpPacket tcp)
                {
                    return new FlowKey()
                    {
                        Protocol = ProtocolType.TCP,
                        SourceEndpoint = new IPEndPoint((tcp.ParentPacket as IpPacket).SourceAddress, tcp.SourcePort),
                        DestinationEndpoint = new IPEndPoint((tcp.ParentPacket as IpPacket).DestinationAddress, tcp.DestinationPort),
                    };
                }
                FlowKey GetIpFlowKey(IpPacket ip)
                {
                    return new FlowKey()
                    {
                        Protocol = ip.Version == IpVersion.IPv4 ? ProtocolType.IP : ProtocolType.IPv6,
                        SourceEndpoint = new IPEndPoint(ip.SourceAddress, 0),
                        DestinationEndpoint = new IPEndPoint(ip.DestinationAddress, 0),
                    };
                }

                switch ((TransportPacket)packet.Extract(typeof(TransportPacket)))
                {
                    case UdpPacket udp: return GetUdpFlowKey(udp);
                    case TcpPacket tcp: return GetTcpFlowKey(tcp);
                    default:
                        switch ((InternetPacket)packet.Extract(typeof(InternetPacket)))
                        {
                            case IpPacket ip: return GetIpFlowKey(ip);
                            default: return FlowKey.None;

                        }
                }
            }
        }
}
