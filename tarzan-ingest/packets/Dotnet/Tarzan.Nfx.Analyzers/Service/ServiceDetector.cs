using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Ignite;
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
            var flowCache = CacheFactory.GetOrCreateFlowCache(m_ignite,m_cacheName);
            var localFlows = flowCache.GetLocalEntries();
            var localFlowCount = flowCache.GetLocalSize();

            m_ignite.Logger.Log(Apache.Ignite.Core.Log.LogLevel.Info,
                $"Compute action {nameof(ServiceDetector)}: Processing {localFlowCount} local flows from {flowCache.Name}.",
                null, null, null, null, null);
            
            var flows = localFlows.Select(entry =>
            {
                var value = entry.Value;
                var conversation = new Conversation<FlowKey> { ConversationKey = entry.Key, Upflow = entry.Key, Downflow = entry.Key.SwapEndpoints() };
                value.ServiceName = m_classifier.Match(conversation)?.ProtocolName;
                return KeyValuePair.Create(entry.Key, value);
            });
            flowCache.PutAll(flows);

            m_ignite.Logger.Log(Apache.Ignite.Core.Log.LogLevel.Info,
                $"Compute action {nameof(ServiceDetector)}: Done.",
                null, null, null, null, null);            
        }
    }
}
