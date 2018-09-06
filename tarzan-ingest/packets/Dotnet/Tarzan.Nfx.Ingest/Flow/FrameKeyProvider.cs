using Netdx.ConversationTracker;
using Netdx.Packets.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest.Flow
{
    public class FrameKeyProvider : IKeyProvider<FlowKey, Frame>
    {
        public FlowKey GetKey(Frame packet)
        {
            return GetKey(packet.Data);
        }
        public static FlowKey GetKey(byte[] bytes)
        {
            var etherType = EthernetFrame.GetEtherType(bytes);
            var etherPayload = EthernetFrame.GetPayloadBytes(bytes);

            Span<Byte> ipPayload = stackalloc byte[0];
            var protocol = 0;
            Span<Byte> sourceAddress = stackalloc byte[4];
            Span<Byte> destinAddress = stackalloc byte[4];
            UInt16 sourcePort = 0;
            UInt16 destinPort = 0;
            switch (etherType)
            {
                case (ushort)EthernetFrame.EtherTypeEnum.Ipv4:
                    {
                        sourceAddress = Ipv4Packet.GetSourceAddress(etherPayload);
                        destinAddress = Ipv4Packet.GetDestinationAddress(etherPayload);
                        ipPayload = Ipv4Packet.GetPayloadBytes(etherPayload);
                        protocol = Ipv4Packet.GetProtocol(etherPayload);
                        break;
                    }
                case (ushort)EthernetFrame.EtherTypeEnum.Ipv6:
                    {
                        sourceAddress = Ipv6Packet.GetSourceAddress(etherPayload);
                        destinAddress = Ipv6Packet.GetDestinationAddress(etherPayload);
                        ipPayload = Ipv6Packet.GetPayloadBytes(etherPayload);
                        protocol = Ipv4Packet.GetProtocol(etherPayload);
                        break;
                    }
                default:
                    break;
            }
            switch (protocol)
            {
                case 6:  // TCP
                    {
                        sourcePort = TcpSegment.GetSourcePort(ipPayload);
                        destinPort = TcpSegment.GetDestinationPort(ipPayload);
                        break;
                    }
                case 17: // UDP
                    {
                        sourcePort = UdpDatagram.GetSourcePort(ipPayload);
                        destinPort = UdpDatagram.GetDestinationPort(ipPayload);
                        break;
                    }
                default:
                    break;
            }
            // ok we have enough information for creating packet's flow key
            return FlowKey.Create((byte)protocol, sourceAddress, sourcePort, destinAddress, destinPort);
        }
    }
}
