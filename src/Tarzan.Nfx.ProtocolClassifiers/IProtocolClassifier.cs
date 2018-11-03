using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.ProtocolClassifiers
{
    /// <summary>
    /// The interface defines common methods of protocol classifiers.
    /// </summary>
    /// <typeparam name="TFlowRecord">A type of flow record. Some classifiers require access to content of packets, their metadata or aggregated flow data.</typeparam>
    public interface IProtocolClassifier<TFlowRecord>
    {
        void LoadConfiguration(string filepath);
        void StoreConfiguration(string filepath);
        void Train(string protocol, Conversation<TFlowRecord> conversation);
        ClassifierMatch Match(Conversation<TFlowRecord> conversation);
        IEnumerable<ClassifierMatch> Matches(Conversation<TFlowRecord> conversation);
    }
}
