using System;
using System.Net;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using PacketDotNet;
using SharpPcap;

namespace Tarzan.Nfx.Ingest
{
    class KeyProvider : IKeyProvider<PacketFlowKey, Packet>
    {
        public PacketFlowKey GetKey(Packet packet)
        {
            PacketFlowKey GetUdpFlowKey(UdpPacket udp)
            {
                return PacketFlowKey.Create((byte)IPProtocolType.UDP,
                    (udp.ParentPacket as IpPacket).SourceAddress.GetAddressBytes(),
                    udp.SourcePort,
                    (udp.ParentPacket as IpPacket).DestinationAddress.GetAddressBytes(),
                    udp.DestinationPort);
            }
            PacketFlowKey GetTcpFlowKey(TcpPacket tcp)
            {
                return PacketFlowKey.Create(
                    (byte)IPProtocolType.TCP,
                    (tcp.ParentPacket as IpPacket).SourceAddress.GetAddressBytes(),
                    tcp.SourcePort,
                    (tcp.ParentPacket as IpPacket).DestinationAddress.GetAddressBytes(),
                    tcp.DestinationPort);
            }
            PacketFlowKey GetIpFlowKey(IpPacket ip)
            {
                return PacketFlowKey.Create(
                    (byte)(ip.Version == IpVersion.IPv4 ? IPProtocolType.IP : IPProtocolType.IPV6),
                    ip.SourceAddress.GetAddressBytes(), 0,
                    ip.DestinationAddress.GetAddressBytes(), 0
                );
            }

            switch ((TransportPacket)packet.Extract(typeof(TransportPacket)))
            {
                case UdpPacket udp: return GetUdpFlowKey(udp);
                case TcpPacket tcp: return GetTcpFlowKey(tcp);
                default:
                    switch ((InternetPacket)packet.Extract(typeof(InternetPacket)))
                    {
                        case IpPacket ip: return GetIpFlowKey(ip);
                        default:
                            return PacketFlowKey.Create((byte)IPProtocolType.NONE,
                       IPAddress.None.GetAddressBytes(), 0, IPAddress.None.GetAddressBytes(), 0);

                    }
            }
        }
    }
}
