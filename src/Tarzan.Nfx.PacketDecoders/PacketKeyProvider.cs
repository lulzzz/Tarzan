using PacketDotNet;
using System.Net;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.PacketDecoders
{
    class PacketKeyProvider : IKeyProvider<FlowKey, Packet>
    {
        public static FlowKey GetKeyFromPacket(Packet packet)
        {
            FlowKey GetUdpFlowKey(UdpPacket udp)
            {
                return FlowKey.Create((byte)IPProtocolType.UDP,
                    (udp.ParentPacket as IPPacket).SourceAddress.GetAddressBytes(),
                    udp.SourcePort,
                    (udp.ParentPacket as IPPacket).DestinationAddress.GetAddressBytes(),
                    udp.DestinationPort);
            }
            FlowKey GetTcpFlowKey(TcpPacket tcp)
            {
                return FlowKey.Create(
                    (byte)IPProtocolType.TCP,
                    (tcp.ParentPacket as IPPacket).SourceAddress.GetAddressBytes(),
                    tcp.SourcePort,
                    (tcp.ParentPacket as IPPacket).DestinationAddress.GetAddressBytes(),
                    tcp.DestinationPort);
            }
            FlowKey GetIpFlowKey(IPPacket ip)
            {
                return FlowKey.Create(
                    (byte)(ip.Version == IPVersion.IPv4 ? IPProtocolType.IP : IPProtocolType.IPV6),
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
                        case IPPacket ip: return GetIpFlowKey(ip);
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
