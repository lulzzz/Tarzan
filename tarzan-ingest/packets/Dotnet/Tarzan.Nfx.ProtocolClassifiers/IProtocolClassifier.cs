using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.ProtocolClassifiers
{
    public interface IProtocolClassifier<TFlowRecord>
    {
        void LoadConfiguration(string filepath);
        void StoreConfiguration(string filepath);
        void Train(string protocol, Conversation<TFlowRecord> conversation);
        ClassifierMatch Match(Conversation<TFlowRecord> conversation);
        IEnumerable<ClassifierMatch> Matches(Conversation<TFlowRecord> conversation);
    }
}
