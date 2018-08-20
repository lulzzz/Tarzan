using System.Collections.Generic;
using System.Text;
using Netdx.ConversationTracker;
using PacketDotNet;
using IPEndPoint = System.Net.IPEndPoint;
using SharpPcap;
using System.Threading;
using System.IO;

namespace Tarzan.Nfx.Ingest
{
    class FlowTable : IFlowTable<FlowKey, FlowRecordWithPackets>, IKeyProvider<FlowKey, (Packet, PosixTimeval)>, IRecordProvider<(Packet, PosixTimeval), FlowRecordWithPackets>
    {
        Dictionary<FlowKey, FlowRecordWithPackets> m_table = new Dictionary<FlowKey, FlowRecordWithPackets>();

        public object Count => m_table.Count;

        public IEnumerable<KeyValuePair<FlowKey, FlowRecordWithPackets>> Entries => m_table;

        public FlowRecordWithPackets Delete(FlowKey key)
        {
            lock (LockObject)
            {
                m_table.Remove(key, out var record);

                return record;
            }
        }

        public bool Exists(FlowKey key)
        {
            return m_table.ContainsKey(key);
        }

        public void FlushAll()
        {
            lock (LockObject)
            {
                m_table.Clear();
            }
        }

        public FlowRecordWithPackets Get(FlowKey key)
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

        /// <summary>
        /// Lock object to control entering to the critical section. 
        /// </summary>
        private readonly object LockObject = new object();

        /// <summary>
        /// Leaves the critical section. 
        /// </summary>
        public void Exit()
        {
            Monitor.Exit(LockObject);    
        }

        /// <summary>
        /// Enters the critical section. 
        /// </summary>
        public void Enter()
        {
            Monitor.Enter(LockObject);
        }

        public FlowRecordWithPackets GetRecord((Packet, PosixTimeval) capture)
        {
            return FlowRecordWithPackets.From(capture);
        }

        public FlowRecordWithPackets Merge(FlowKey key, FlowRecordWithPackets value)
        {

            var stored = m_table.GetValueOrDefault(key);
            FlowRecordWithPackets newValue;
            if (stored != null)
            {
                newValue = FlowRecordWithPackets.Merge(stored, value);
            }
            else
            {
                newValue = value;
            }
            lock (LockObject)
            {
                return m_table[key] = newValue;
            }
        }

        public void Put(FlowKey key, FlowRecordWithPackets value)
        {
            lock (LockObject)
            {
                m_table[key] = value;
            }
        }


        public void Write(Stream stream)
        {

        }

        public void Read(Stream stream)
        {
            
        }
    }
}
