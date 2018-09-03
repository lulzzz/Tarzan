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

        public IPEndPoint Endpoint => m_endpoint;
        public string Keyspace => m_keyspace;

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

        public void WriteCapture(FileInfo fileinfo)
        {
            var capture = new Capture
            {
                Uid = Guid.NewGuid().ToString(),
                CreationTime = new DateTimeOffset(fileinfo.CreationTime).ToUnixTimeMilliseconds(),
                Name = fileinfo.Name,
                Length = fileinfo.Length,
                Hash = "",
            };
            m_dataset.CaptureTable.Insert(capture).Execute();
        }

        public void WriteFlow(FlowKey flowKey, PacketStream flowValue)
        {
            var uid = PacketFlow.NewUid(flowKey.Protocol.ToString(), flowKey.SourceEndpoint, flowKey.DestinationEndpoint, flowValue.FirstSeen);
            var flowPoco = new PacketFlow
            {
                Uid = uid.ToString(),
                Protocol = flowKey.Protocol.ToString(),
                SourceAddress = flowKey.SourceEndpoint.Address.ToString(),
                SourcePort = flowKey.SourceEndpoint.Port,
                DestinationAddress = flowKey.DestinationEndpoint.Address.ToString(),
                DestinationPort = flowKey.DestinationEndpoint.Port,
                FirstSeen = flowValue.FirstSeen,
                LastSeen = flowValue.LastSeen,
                Octets = flowValue.Octets,
                Packets = flowValue.Packets
            };
            m_dataset.FlowTable.Insert(flowPoco).Execute();
            var objectPoco = new AffObject
            {
                ObjectName = flowPoco.ObjectName,
                ObjectType = nameof(PacketFlow)
            };

            m_dataset.CatalogueTable.Insert(objectPoco).Execute();

        }
        public void WriteHosts(IEnumerable<Tarzan.Nfx.Model.Host> hosts)
        {
            foreach (var host in hosts)
            {
                var insert = m_dataset.HostTable.Insert(host);
                insert.Execute();
            }
        }

       
        public void WriteServices(IEnumerable<Service> services)
        {
            foreach(var service in services)
            {
                var insert = m_dataset.ServiceTable.Insert(service);
                insert.Execute();
            }
        }

        public void WriteDns(IEnumerable<DnsObject> dnsObjects)
        {
            foreach(var dnsObject in dnsObjects)
            {
                var insert = m_dataset.DnsTable.Insert(dnsObject);
                insert.Execute();
                var objectPoco = new AffObject
                {
                    ObjectName = dnsObject.ObjectName,
                    ObjectType = nameof(DnsObject)
                };
                m_dataset.CatalogueTable.Insert(objectPoco).Execute();
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



        public void WriteHttp(IEnumerable<HttpObject> httpObjects)
        {
            foreach (var httpInfo in httpObjects)
            {
                var insert = m_dataset.HttpTable.Insert(httpInfo);
                insert.Execute();
                var objectPoco = new AffObject
                {
                    ObjectName = httpInfo.ObjectName,
                    ObjectType = nameof(HttpObject)
                };
                m_dataset.CatalogueTable.Insert(objectPoco).Execute();
            }
        }
    }
}
