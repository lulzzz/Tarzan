using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.ProtocolClassifiers.Statistical
{
    public class StatisticalClassifier : IProtocolClassifier<FlowTracker.FlowRecord>
    {
        public void LoadConfiguration(string filepath)
        {
            throw new NotImplementedException();
        }

        public ClassifierMatch Match(Conversation<FlowRecord> conversation)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClassifierMatch> Matches(Conversation<FlowRecord> conversation)
        {
            throw new NotImplementedException();
        }

        public void StoreConfiguration(string filepath)
        {
            throw new NotImplementedException();
        }

        public void Train(string protocol, Conversation<FlowRecord> conversation)
        {
            throw new NotImplementedException();
        }
    }
}
