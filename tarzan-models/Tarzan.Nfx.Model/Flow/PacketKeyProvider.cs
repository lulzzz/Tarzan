using PacketDotNet;
using System.Net;

namespace Tarzan.Nfx.Model
{
    class PacketKeyProvider : IKeyProvider<FlowKey, Packet>
    {
        public static FlowKey GetKeyFromPacket(Packet packet)
        {
            FlowKey GetUdpFlowKey(UdpPacket udp)
            {
                return FlowKey.Create((byte)IPProtocolType.UDP,
                    (udp.ParentPacket as IpPacket).SourceAddress.GetAddressBytes(),
                    udp.SourcePort,
                    (udp.ParentPacket as IpPacket).DestinationAddress.GetAddressBytes(),
                    udp.DestinationPort);
            }
            FlowKey GetTcpFlowKey(TcpPacket tcp)
            {
                return FlowKey.Create(
                    (byte)IPProtocolType.TCP,
                    (tcp.ParentPacket as IpPacket).SourceAddress.GetAddressBytes(),
                    tcp.SourcePort,
                    (tcp.ParentPacket as IpPacket).DestinationAddress.GetAddressBytes(),
                    tcp.DestinationPort);
            }
            FlowKey GetIpFlowKey(IpPacket ip)
            {
                return FlowKey.Create(
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
                            return FlowKey.Create((byte)IPProtocolType.NONE,
                       IPAddress.None.GetAddressBytes(), 0, IPAddress.None.GetAddressBytes(), 0);

                    }
            }
        }

        public FlowKey GetKey(Packet packet)
        {
            return GetKeyFromPacket(packet);
        }
    }
}
