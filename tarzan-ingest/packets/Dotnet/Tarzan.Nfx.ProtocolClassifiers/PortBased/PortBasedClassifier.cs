using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.ProtocolClassifiers.PortBased
{
    public class PortBasedClassifier : IProtocolClassifier<FlowKey>
    {
        Dictionary<string, ServiceName> m_serviceDictionary;

        class ServiceName
        {
            public int Port { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Protocol { get; set; }
        }

        string DetectService(FlowKey packetFlow)
        {
            if (m_serviceDictionary == null) throw new InvalidOperationException("Service dictionary not loaded.");
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
            var serviceName = getServiceName(packetFlow.Protocol.ToString(), Math.Min(packetFlow.SourcePort, packetFlow.DestinationPort));
            return serviceName;
        }

        public void LoadConfiguration(string filepath)
        {
            var dictionary = new Dictionary<string, ServiceName>();
            Stream resource = null;
            if (filepath == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                resource = assembly.GetManifestResourceStream("Tarzan.Nfx.ProtocolClassifiers.PortBased.Resources.service-names-port-numbers.csv");
            }
            else
            {
                resource = File.Open(filepath, FileMode.Open);
            }
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
        }

        public void StoreConfiguration(string filepath)
        {
            throw new NotSupportedException();
        }


        public void Train(string protocol,  Conversation<FlowKey> conversation)
        {
            throw new NotImplementedException();
        }

        public ClassifierMatch Match(Conversation<FlowKey> conversation)
        {
            var protocolName = DetectService(conversation.ConversationKey);
            return new ClassifierMatch
            {
                ProtocolName = protocolName,
                Similarity = 1
            };
        }

        public IEnumerable<ClassifierMatch> Matches(Conversation<FlowKey> conversation)
        {
            yield return Match(conversation);
        }
    }
}
