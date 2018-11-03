using Tarzan.Nfx.Packets.Base;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.PacketDecoders
{
    /// <summary>
    /// Provides a flow key from the given frame. This class provides optimized 
    /// fast method for getting flow key from Ethernet frames only.
    /// For general method to get flow key from any other link type use <see cref="PacketKeyProvider"/> class.
    /// </summary>
    public class FrameKeyProvider : IKeyProvider<FlowKey, FrameData>
    {                    
        /// <summary>
        /// Gets the flow key for the given frame.
        /// </summary>
        /// <param name="frame">Frame object.</param>
        /// <returns>The flow key for the passed frame.</returns>
        public FlowKey GetKey(FrameData frame)
        {
            if (frame.LinkLayer == LinkLayerType.Ethernet)
            {
                return GetKey(frame.Data);
            }
            else
            {
                return GetKey(frame.LinkLayer, frame.Data);
            }
        }

        public int GetKeyHash(FrameData frame)
        {
            var frameKey = GetKey(frame);
            return frameKey.FlowKeyHash;
        }

        /// <summary>
        /// Gets the key for Ethernet frame.
        /// </summary>
        /// <param name="frameBytes">Bytes that contains Ethernet frame.</param>
        /// <returns><see cref="FlowKey"/> for the provided Ethernet frame.</returns>
        public static FlowKey GetKey(byte[] frameBytes)
        {
            var etherType = EthernetFrame.GetEtherType(frameBytes);
            var etherPayload = EthernetFrame.GetPayloadBytes(frameBytes);

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
                        protocol = Ipv6Packet.GetProtocol(etherPayload);
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

        /// <summary>
        /// Gets the flow key for supported frame (not only Ethernet). It uses PacketDotNet library.
        /// </summary>
        /// <param name="linkLayer">Link Layer of the Frame.</param>
        /// <param name="bytes">Bytes of the frame.</param>
        /// <returns><see cref="FlowKey"/> for the provided frame.</returns>
        public static FlowKey GetKey(LinkLayerType linkLayer, byte[] bytes)
        {
            var packet = PacketDotNet.Packet.ParsePacket((PacketDotNet.LinkLayers)linkLayer, bytes);
            return PacketKeyProvider.GetKeyFromPacket(packet);
        }
    }
}
