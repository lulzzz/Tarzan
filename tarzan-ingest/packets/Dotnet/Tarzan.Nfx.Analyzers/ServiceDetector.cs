using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.ProtocolClassifiers.PortBased;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public class ServiceDetector : IComputeAction
    {
        private readonly PortBasedClassifier m_classifier;
        public ServiceDetector()
        {
            m_classifier = new PortBasedClassifier();
            m_classifier.LoadConfiguration(null);
        }
         
        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public void Invoke()
        {
            var flowCache = m_ignite.GetCache<FlowKey, PacketFlow>(PacketFlow.CACHE_NAME); 
            var localFlows = flowCache.GetLocalEntries();
            var localFlowCount = flowCache.GetLocalSize();

            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]   INGEST: ServiceDetector: Processing {localFlowCount} local flows.");

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
