using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    public class ServiceDetector : IComputeAction
    {
        Dictionary<string, ServiceName> m_serviceDictionary;
        public ServiceDetector()
        {
            LoadServiceNames();
        }

        class ServiceName
        {
            public int Port { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Protocol { get; set; }
        }

        private void LoadServiceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceStream("Tarzan.Nfx.Ingest.service-names-port-numbers.csv");
            var dictionary = new Dictionary<string, ServiceName>();
            using (var tr = new StreamReader(resource))
            {
                for (var line = tr.ReadLine(); line != null; line = tr.ReadLine())
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
                m_serviceDictionary = dictionary;
            }
            // load CSV file:

        }

        public string DetectService(PacketFlowKey flowKey, PacketStream FlowValue)
        {
            string getServiceName(string protocol, int port)
            {
                if (m_serviceDictionary.TryGetValue($"{protocol.ToLowerInvariant()}/{port}", out var service))
                {
                    return service.Name;
                }
                else
                {
                    return $"{protocol.ToLowerInvariant()}/{port}";
                }
            }
            var serviceName = getServiceName(flowKey.Protocol.ToString(), Math.Min(flowKey.SourcePort, flowKey.DestinationPort));
            return serviceName;
        }

        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public void Invoke()
        {
            var cache = new FlowCache(m_ignite);
            foreach (var flow in cache.GetLocalEntries())
            {
                flow.Value.ServiceName = DetectService(flow.Key, flow.Value);
                cache.Put(flow.Key, flow.Value);
            }
        }
    }
}
