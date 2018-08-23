using System.Collections.Generic;
using System.Text;
using Netdx.ConversationTracker;
using PacketDotNet;
using IPEndPoint = System.Net.IPEndPoint;
using SharpPcap;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

namespace Tarzan.Nfx.Ingest
{
    class FlowTable : IFlowTable<FlowKey, PacketStream>, IKeyProvider<FlowKey, (Packet, PosixTimeval)>, IRecordProvider<(Packet, PosixTimeval), PacketStream>
    {
        ConcurrentDictionary<FlowKey, PacketStream> m_table = new ConcurrentDictionary<FlowKey, PacketStream>();

        public IEnumerable<KeyValuePair<FlowKey, PacketStream>> Entries => m_table;


        public int Count => m_table.Count;

        public PacketStream Delete(FlowKey key)
        {
                m_table.Remove(key, out var record);
                return record;
        }

        public bool Exists(FlowKey key)
        {
            return m_table.ContainsKey(key);
        }

        public void FlushAll()
        {
                m_table.Clear();
        }

        public PacketStream Get(FlowKey key)
        {
            return m_table.GetValueOrDefault(key);
        }

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

        public PacketStream GetRecord((Packet, PosixTimeval) capture)
        {
            return PacketStream.From(capture);
        }

        public PacketStream Merge(FlowKey key, PacketStream value)
        {
            return m_table.AddOrUpdate(key, value, (k, v) => PacketStream.Merge(v, value));
        }

        public void Put(FlowKey key, PacketStream value)
        {
            m_table[key] = value;
        }
    }
}
