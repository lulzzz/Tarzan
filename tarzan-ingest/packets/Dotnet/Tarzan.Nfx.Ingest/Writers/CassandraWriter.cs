using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Netdx.PacketDecoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tarzan.Nfx.Ingest.Analyzers;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Model.Cassandra;

namespace Tarzan.Nfx.Ingest
{
    partial class CassandraWriter
    {
        private IPEndPoint m_endpoint;
        private string m_keyspace;
        private AffDataset m_dataset;

        public CassandraWriter(IPEndPoint endpoint, string keyspace)
        {
            this.m_endpoint = endpoint;
            this.m_keyspace = keyspace;
            m_dataset = new AffDataset(m_endpoint, m_keyspace);
        }

        public void DeleteKeyspace()
        {
            AffDataset.DeleteKeyspace(m_endpoint, m_keyspace);
        }

        public void Initialize()
        {
            m_dataset.Connect();
        }

        public void Shutdown()
        {
            m_dataset.Close();
        }

        public async Task WriteAll(FileInfo fileinfo, IDictionary<PacketFlowKey, PacketStream> table)
        {
            Console.WriteLine($"{DateTime.Now} Writing CAPTURES...");
            await WriteCapture(fileinfo);
            Console.WriteLine($"{DateTime.Now} Writing FLOWS...");
            await WriteFlows(table);
            Console.WriteLine($"{DateTime.Now} Writing HOSTS...");
            await WriteHosts(table);
            Console.WriteLine($"{DateTime.Now} Writing SERVICES...");
            await WriteServices(table);
            Console.WriteLine($"{DateTime.Now} Writing DNS...");
            await WriteDns(table);
            Console.WriteLine($"{DateTime.Now} Writing HTTP...");
            await WriteHttp(table);
        }

        async Task WriteCapture(FileInfo fileinfo)
        {
            var capture = new Capture
            {
                Uid = Guid.NewGuid().ToString(),
                CreationTime = new DateTimeOffset(fileinfo.CreationTime).ToUnixTimeMilliseconds(),
                Name = fileinfo.Name,
                Length = fileinfo.Length,
                Hash = "",
            };
            await m_dataset.CaptureTable.Insert(capture).ExecuteAsync();
        }

        async Task WriteFlows(IDictionary<PacketFlowKey, PacketStream> table)
        {

            foreach (var flow in table)
            {
                var uid = PacketFlow.NewUid(flow.Key.Protocol.ToString(), flow.Key.SourceEndpoint, flow.Key.DestinationEndpoint, flow.Value.FirstSeen);                    
                var flowPoco = new PacketFlow
                {
                    Uid = uid.ToString(),
                    Protocol = flow.Key.Protocol.ToString(),
                    SourceAddress = flow.Key.SourceEndpoint.Address.ToString(),
                    SourcePort = flow.Key.SourceEndpoint.Port,
                    DestinationAddress = flow.Key.DestinationEndpoint.Address.ToString(),
                    DestinationPort = flow.Key.DestinationEndpoint.Port,
                    FirstSeen = flow.Value.FirstSeen,
                    LastSeen = flow.Value.LastSeen,
                    Octets = flow.Value.Octets,
                    Packets = flow.Value.Packets
                };
                await m_dataset.FlowTable.Insert(flowPoco).ExecuteAsync();
                var objectPoco = new AffObject
                {
                    ObjectName = flowPoco.ObjectName,
                    ObjectType = nameof(PacketFlow)
                };
                
                await m_dataset.CatalogueTable.Insert(objectPoco).ExecuteAsync();
            }
        }
        async Task WriteHosts(IDictionary<PacketFlowKey, PacketStream> table)
        {
            var srcHosts = table.GroupBy(x => x.Key.SourceEndpoint.Address).Select(t =>
                new Model.Host { Address = t.Key.ToString(), UpFlows = t.Count(), PacketsSent = t.Sum(p => p.Value.Packets), OctetsSent = t.Sum(p => p.Value.Octets) });

            var dstHosts = table.GroupBy(x => x.Key.DestinationEndpoint.Address).Select(t =>
                new Model.Host { Address = t.Key.ToString(), DownFlows = t.Count(), PacketsRecv = t.Sum(p => p.Value.Packets), OctetsRecv = t.Sum(p => p.Value.Octets) });

            foreach (var host in srcHosts)
            {
                var insert = m_dataset.HostTable.Insert(host);
                await insert.ExecuteAsync();
            }
            
            foreach (var host in dstHosts)
            {
                await m_dataset.Mapper.UpdateAsync<Model.Host>("SET downflows=?, octetsRecv=?, packetsRecv=? WHERE address=?", host.DownFlows, host.OctetsRecv, host.PacketsRecv, host.Address);
            }
        }

       
        private async Task WriteServices(IDictionary<PacketFlowKey, PacketStream> table)
        {
            var services = table
                .GroupBy(f => f.Value.ServiceName)
                .Select(f =>
                    new Model.Service
                    {
                        ServiceName = f.Key,
                        Flows = f.Count(),
                        Packets = f.Sum(x => x.Value.Packets),
                        MaxPackets = f.Max(x => x.Value.Packets),
                        MinPackets = f.Min(x => x.Value.Packets),
                        Octets = f.Sum(x => x.Value.Octets),
                        MaxOctets = f.Max(x => x.Value.Octets),
                        MinOctets = f.Min(x => x.Value.Octets),
                        AvgDuration = (long)f.Average(x => x.Value.LastSeen - x.Value.FirstSeen),
                        MaxDuration = f.Max(x => x.Value.LastSeen - x.Value.FirstSeen),
                        MinDuration = f.Min(x => x.Value.LastSeen - x.Value.FirstSeen),
                    }); 
            foreach(var service in services)
            {
                var insert = m_dataset.ServiceTable.Insert(service);
                await insert.ExecuteAsync();
            }
        }

        private async Task WriteDns(IDictionary<PacketFlowKey, PacketStream> table)
        {
            var dnsInfos = table.Where(f=>f.Value.ServiceName.Equals("domain", StringComparison.InvariantCultureIgnoreCase)).SelectMany(x => DnsAnalyzer.Inspect(x.Key, x.Value));
            foreach(var dnsInfo in dnsInfos)
            {
                var insert = m_dataset.DnsTable.Insert(dnsInfo);
                await insert.ExecuteAsync();
                var objectPoco = new AffObject
                {
                    ObjectName = dnsInfo.ObjectName,
                    ObjectType = nameof(DnsObject)
                };
                await m_dataset.CatalogueTable.Insert(objectPoco).ExecuteAsync();
            }
        }

        private void DebugSeqnum(Guid flowId, TcpStream stream)
        {
            var contents = "timeval, SYN, FIN, RST, ACK, PSH, Seq, Ack, Len, Exp"+Environment.NewLine;
            foreach(var packet in TcpStream.SegmentMap(stream))
            {
                var nextSeqNum = packet.SequenceNumber + packet.Length + (packet.Syn || packet.Fin || packet.Rst ? 1 : 0);
                contents += $"{packet.Timeval}, {packet.Syn}, {packet.Fin}, {packet.Rst}, {packet.Ack}, {packet.Psh}, {packet.SequenceNumber}, {packet.AcknowledgmentNumber}, {packet.Length}, {nextSeqNum}"+Environment.NewLine;
            }

            File.WriteAllText(flowId.ToString() + ".csv", contents);
        }

        

        private async Task WriteHttp(IDictionary<PacketFlowKey, PacketStream> table)
        {
            var httpFlows = table.Where(f => f.Value.ServiceName.Equals("www-http", StringComparison.InvariantCultureIgnoreCase));
            var httpStreams = TcpStream.Split(httpFlows).ToList();
            //httpStreams.ForEach(x=> DebugSeqnum(PacketFlow.NewUid(x.Key.Protocol.ToString(), x.Key.SourceEndpoint, x.Key.DestinationEndpoint, x.Value.FirstSeen), x.Value));
            var httpInfos = TcpStream.Pair(httpStreams).SelectMany(c => HttpAnalyzer.Inspect(c));
            foreach (var httpInfo in httpInfos)
            {
                var insert = m_dataset.HttpTable.Insert(httpInfo);
                await insert.ExecuteAsync();
                var objectPoco = new AffObject
                {
                    ObjectName = httpInfo.ObjectName,
                    ObjectType = nameof(HttpObject)
                };
                await m_dataset.CatalogueTable.Insert(objectPoco).ExecuteAsync();
            }
        }
    }
}
