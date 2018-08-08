using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Tarzan.Nfx;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest
{
    class CassandraWriter
    {
        private IPEndPoint m_endpoint;
        private string m_keyspace;
        private ISession m_session;
        private IMapper m_mapper;
        private Table<Model.Flow> m_flowTable;
        private Table<Model.Host> m_hostTable;
        private Table<Model.Service> m_serviceTable;

        public CassandraWriter(IPEndPoint endpoint, string keyspace)
        {
            this.m_endpoint = endpoint;
            this.m_keyspace = keyspace;
        }

        internal void Setup()
        {
            Tarzan.Nfx.Model.Cassandra.ModelMapping.Register(MappingConfiguration.Global);

            var cluster = Cluster.Builder().AddContactPoint(m_endpoint).WithDefaultKeyspace(m_keyspace).Build();     
            m_session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            m_mapper = new Mapper(m_session);

            m_flowTable = new Table<Model.Flow>(m_session);
            m_flowTable.CreateIfNotExists();
            m_hostTable = new Table<Model.Host>(m_session);
            m_hostTable.CreateIfNotExists();
            m_serviceTable = new Table<Model.Service>(m_session);
            m_serviceTable.CreateIfNotExists();
        }

        internal void Write(FlowTable table)
        {
            WriteFlows(table);
            WriteHosts(table);
            WriteServices(table);
        }

        internal void Shutdown()
        {
            m_session.Cluster.Shutdown();
        }


        void WriteFlows(FlowTable table)
        {
            foreach (var (flow, index) in table.Entries.Select(x => (x, Guid.NewGuid())))
            {
                var flowPoco = new Flow
                {
                    FlowId = index.ToString(),
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

        class ServiceName
        {
            public int Port { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Protocol { get; set; }
        }

        private Dictionary<string, ServiceName> LoadServiceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceStream("Tarzan.Nfx.Ingest.service-names-port-numbers.csv");
            var dictionary = new Dictionary<string, ServiceName>();
            using (var tr = new StreamReader(resource))
            {
                for(var line = tr.ReadLine(); line != null; line = tr.ReadLine())
                {
                    var components = line.Split(',');
                    if (components.Length < 4) continue;
                    var name = components[0];
                    var port = components[1];
                    var protocol = components[2];
                    var description = components[3];
                    if (!String.IsNullOrWhiteSpace(port) && Int32.TryParse(port, out int protocolNumber))
                    {
                        var key = $"{protocol.ToLowerInvariant()}/{port}";
                        dictionary[key] = new ServiceName
                        {
                            Name = String.IsNullOrWhiteSpace(name) ? key : name,
                            Port = protocolNumber,
                            Protocol = protocol,
                            Description = description
                        };
                    }
                }
                return dictionary;
            }
            // load CSV file:

        }

        private void WriteServices(FlowTable table)
        {
            var serviceDict = LoadServiceNames();
            string getServiceName(string protocol, int port)
            {
                if (serviceDict.TryGetValue($"{protocol.ToLowerInvariant()}/{port}", out var service))
                {
                    return service.Name;
                }
                else
                {
                    return $"{protocol.ToLowerInvariant()}/{port}";
                }
            }

            var services = table.Entries
                .GroupBy(f => ( f.Key.Protocol, Port: Math.Min(f.Key.SourceEndpoint.Port, f.Key.DestinationEndpoint.Port)))
                .Select(f =>
                    new Model.Service
                    {
                        Name = getServiceName(f.Key.Protocol.ToString(), f.Key.Port),
                        Flows = f.Count(),
                        Packets = f.Sum(x=>x.Value.Packets),
                        MaxPackets = f.Max(x => x.Value.Packets),
                        MinPackets = f.Min(x => x.Value.Packets),
                        Octets = f.Sum(x=>x.Value.Octets),
                        MaxOctets = f.Max(x => x.Value.Octets),
                        MinOctets = f.Min(x => x.Value.Octets),
                        AvgDuration = (long)f.Average(x=>x.Value.LastSeen-x.Value.FirstSeen),
                        MaxDuration = f.Max(x => x.Value.LastSeen - x.Value.FirstSeen),
                        MinDuration = f.Min(x => x.Value.LastSeen - x.Value.FirstSeen),                        
                    }); 
            foreach(var service in services)
            {
                var insert = m_serviceTable.Insert(service);
                insert.Execute();
            }
        }
    }
}
