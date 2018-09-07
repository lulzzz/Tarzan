using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.Ingest;
using Tarzan.Nfx.Ingest.Flow;

namespace PacketDecodersTest
{
    [CoreJob]
    public class ComputeFlowsBenchmark
    {

        [Params(@"Resources\http.cap")]
        public string filename;

        [GlobalSetup]
        public void Setup()
        {
            // read packet from input file:
            _packets = new List<RawCapture>();
            var device = new CaptureFileReaderDevice(filename);
            device.Open();
            RawCapture packet;
            while ((packet = device.GetNextPacket()) != null)
            {
                _packets.Add(packet);
            }
            device.Close();
        }



        public FlowKey GetKey(Packet packet)
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
                        default: return FlowKey.Create((byte)IPProtocolType.NONE, 
                            IPAddress.None.GetAddressBytes(), 0, IPAddress.None.GetAddressBytes(), 0);

                    }
            }
        }

        List<RawCapture> _packets;


        [Benchmark]
        public void LinqPacketDotNet()
        {
            var flows = from packet in _packets.Select(p => (Key: GetKey(Packet.ParsePacket(p.LinkLayerType, p.Data)), Packet: p))
                        group packet by packet.Key;
            var result = flows.ToList();

        }        
        [Benchmark]
        public void LinqFastDecode()
        {
            var flows = from packet in _packets.Select(p => (Key: FrameKeyProvider.GetKey(p.Data), Packet: p))
                        group packet by packet.Key;
            var result = flows.ToList();
        }
        [Benchmark]
        public void PLinqFastDecode()
        {
            var result = _packets.AsParallel().Select(p => (Key: FrameKeyProvider.GetKey(p.Data), Packet: p)).GroupBy(x => x.Key).ToList();
        }


        [Benchmark]
        public void LinqFastDecodeStringKey()
        {
            var flows = from packet in _packets.Select(p => (Key: FrameKeyProvider.GetKey(p.Data).ToString(), Packet: p))
                        group packet by packet.Key;
            var result = flows.ToList();
        }

        [Benchmark]
        public void DictionaryPacketDotNet()
        {
            var flows = new Dictionary<FlowKey, List<RawCapture>>();
            foreach(var packet in _packets)
            {
                var key = GetKey(Packet.ParsePacket(packet.LinkLayerType, packet.Data));
                if (flows.TryGetValue(key, out var lst))
                {
                    lst.Add(packet);
                }
                else
                {
                    flows[key] = new List<RawCapture> { packet };
                }
            }
        }

       
        [Benchmark]
        public void DictionaryFastDecode()
        {
            var flows = new Dictionary<FlowKey, List<RawCapture>>();
            foreach (var packet in _packets)
            {
                var key = FrameKeyProvider.GetKey(packet.Data);
                if (flows.TryGetValue(key, out var lst))
                {
                    lst.Add(packet);
                }
                else
                {
                    flows[key] = new List<RawCapture> { packet };
                }
            }
        }
        [Benchmark]
        public void DictionaryFastDecodeLinkedList()
        {
            var flows = new Dictionary<FlowKey, LinkedList<RawCapture>>();
            foreach (var packet in _packets)
            {
                var key = FrameKeyProvider.GetKey(packet.Data);
                if (flows.TryGetValue(key, out var lst))
                {
                    lst.AddLast(packet);
                }
                else
                {
                    var ll = new LinkedList<RawCapture>();
                    ll.AddLast(packet);
                    flows[key] = ll;
                }
            }
        }

        [Benchmark]
        public void ConcurrentDictionaryFastDecode()
        {
            var flows = new ConcurrentDictionary<FlowKey, List<RawCapture>>();
            foreach (var packet in _packets)
            {
                var key = FrameKeyProvider.GetKey(packet.Data);
                flows.AddOrUpdate(key, new List<RawCapture> { packet },
                    (k, lst) =>
                    {
                        lst.Add(packet);
                        return lst;
                    });
            }
        }

        [Benchmark]
        public void ConcurrentDictionaryParallelFastDecode()
        {
            var flows = new ConcurrentDictionary<FlowKey, ConcurrentBag<RawCapture>>();
            Parallel.ForEach(_packets, (packet) =>
            {
                var key = FrameKeyProvider.GetKey(packet.Data);
                flows.AddOrUpdate(key, 
                    // add method
                    k =>  new ConcurrentBag<RawCapture> { packet },
                    // update method
                    (_, lst) =>
                    {
                        lst.Add(packet);
                        return lst;
                    });
            });
        }

        public class AllowNonOptimized : ManualConfig
        {
            public AllowNonOptimized()
            {
                Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS
                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
            }
        }
    }
}
