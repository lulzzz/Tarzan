using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using System;
using System.IO;
using System.Linq;
using Tarzan.Nfx.Ingest.Analyzers;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest
{
    partial class CassandraWriter
    {
        private IPEndPoint m_endpoint;
        private string m_keyspace;
        private ISession m_session;
        private IMapper m_mapper;
        private Table<Model.PacketFlow> m_flowTable;
        private Table<Model.Host> m_hostTable;
        private Table<Model.Service> m_serviceTable;
        private Table<Model.DnsInfo> m_dnsTable;
        private Table<Model.HttpInfo> m_httpTable;

        public CassandraWriter(IPEndPoint endpoint, string keyspace)
        {
            this.m_endpoint = endpoint;
            this.m_keyspace = keyspace;
        }

        internal void DeleteKeyspace()
        {
            var cluster = Cluster.Builder().AddContactPoint(m_endpoint).Build();
            using (var session = cluster.Connect())
            {
                session.Execute($"DROP KEYSPACE IF EXISTS {m_keyspace}");
            }
        }

        internal void Setup()
        {
            Model.Cassandra.ModelMapping.AutoRegister(MappingConfiguration.Global);

            var cluster = Cluster.Builder().AddContactPoint(m_endpoint).WithDefaultKeyspace(m_keyspace).Build();     
            m_session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            m_mapper = new Mapper(m_session);

            m_flowTable = new Table<Model.PacketFlow>(m_session);
            m_flowTable.CreateIfNotExists();
            m_hostTable = new Table<Model.Host>(m_session);
            m_hostTable.CreateIfNotExists();
            m_serviceTable = new Table<Model.Service>(m_session);
            m_serviceTable.CreateIfNotExists();
            m_dnsTable = new Table<Model.DnsInfo>(m_session);
            m_dnsTable.CreateIfNotExists();
            m_httpTable = new Table<Model.HttpInfo>(m_session);
            m_httpTable.CreateIfNotExists();
        }

        internal void Write(FlowTable table)
        {
            WriteFlows(table);
            WriteHosts(table);
            WriteServices(table);
            WriteDns(table);
            WriteHttp(table);
        }

        internal void Shutdown()
        {
            m_session.Cluster.Shutdown();
        }


        void WriteFlows(FlowTable table)
        {
            foreach (var flow in table.Entries)
            {
                var flowPoco = new PacketFlow
                {
                    FlowId = flow.Value.FlowId.ToString(),
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
                var insert = m_flowTable.Insert(flowPoco);
                insert.Execute();
            }
        }
        void WriteHosts(FlowTable table)
        {
            var srcHosts = table.Entries.GroupBy(x => x.Key.SourceEndpoint.Address).Select(t =>
                new Model.Host { Address = t.Key.ToString(), UpFlows = t.Count(), PacketsSent = t.Sum(p => p.Value.Packets), OctetsSent = t.Sum(p => p.Value.Octets) });

            var dstHosts = table.Entries.GroupBy(x => x.Key.DestinationEndpoint.Address).Select(t =>
                new Model.Host { Address = t.Key.ToString(), DownFlows = t.Count(), PacketsRecv = t.Sum(p => p.Value.Packets), OctetsRecv = t.Sum(p => p.Value.Octets) });

            foreach (var host in srcHosts)
            {
                var insert = m_hostTable.Insert(host);
                insert.Execute();
            }
            
            foreach (var host in dstHosts)
            {
                m_mapper.Update<Model.Host>("SET downflows=?, octetsRecv=?, packetsRecv=? WHERE address=?", host.DownFlows, host.OctetsRecv, host.PacketsRecv, host.Address);
            }
        }

       
        private void WriteServices(FlowTable table)
        {
            var services = table.Entries
                .GroupBy(f => f.Value.ServiceName)
                .Select(f =>
                    new Model.Service
                    {
                        Name = f.Key,
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
                var insert = m_serviceTable.Insert(service);
                insert.Execute();
            }
        }

        private void WriteDns(FlowTable flowTable)
        {
            var dnsInfos = flowTable.Entries.Where(f=>f.Value.ServiceName.Equals("domain", StringComparison.InvariantCultureIgnoreCase)).SelectMany(x => DnsAnalyzer.Inspect(x.Key, x.Value));
            foreach(var dnsInfo in dnsInfos)
            {
                var insert = m_dnsTable.Insert(dnsInfo);
                insert.Execute();
            }
        }

        private void DebugSeqnum(TcpStream stream)
        {
            var contents = "timeval, SYN, FIN, RST, ACK, PSH, Seq, Ack, Len, Exp"+Environment.NewLine;
            foreach(var packet in TcpStream.SegmentMap(stream))
            {
                var nextSeqNum = packet.SequenceNumber + packet.Length + (packet.Syn || packet.Fin || packet.Rst ? 1 : 0);
                contents += $"{packet.Timeval.Date}, {packet.Syn}, {packet.Fin}, {packet.Rst}, {packet.Ack}, {packet.Psh}, {packet.SequenceNumber}, {packet.AcknowledgmentNumber}, {packet.Length}, {nextSeqNum}"+Environment.NewLine;
            }
            File.WriteAllText(stream.FlowId.ToString() + ".csv", contents);
        }

        

        private void WriteHttp(FlowTable flowTable)
        {
            var httpFlows = flowTable.Entries.Where(f => f.Value.ServiceName.Equals("www-http", StringComparison.InvariantCultureIgnoreCase));
            var httpStreams = TcpStream.Split(httpFlows).ToList();
            httpStreams.ForEach(x=> DebugSeqnum(x.Value));
            var httpInfos = TcpStream.Pair(httpStreams).SelectMany(c => HttpAnalyzer.Inspect(c));
            foreach (var httpInfo in httpInfos)
            {
                var insert = m_httpTable.Insert(httpInfo);
                insert.Execute();
            }
        }
    }
}
