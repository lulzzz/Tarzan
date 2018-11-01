using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.ProtocolClassifiers.PortBased;

namespace Tarzan.Nfx.Analyzers
{
    public class ServiceDetector : IComputeAction
    {
        private readonly PortBasedClassifier m_classifier;
        public ServiceDetector(string cacheName)
        {
            m_classifier = new PortBasedClassifier();
            m_classifier.LoadConfiguration(null);
            m_cacheName = cacheName;
        }
         
        [InstanceResource]
        protected readonly IIgnite m_ignite;
        private readonly string m_cacheName;

        public void Invoke()
        {
            var flowCache = m_ignite.GetCache<FlowKey, PacketFlow>(m_cacheName);
            var localFlows = flowCache.GetLocalEntries();
            var localFlowCount = flowCache.GetLocalSize();

            Console.WriteLine($"ServiceDetector: Processing {localFlowCount} local flows from {flowCache.Name}.");

            var flows = localFlows.Select(x =>
            {
                var value = x.Value;
                var conv = new Conversation<FlowKey> { ConversationKey = x.Key, Upflow = x.Key, Downflow = x.Key.SwapEndpoints() };
                value.ServiceName = m_classifier.Match(conv)?.ProtocolName;
                return KeyValuePair.Create(x.Key, value);
            });
            flowCache.PutAll(flows);
        }
    }
}
