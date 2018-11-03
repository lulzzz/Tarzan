using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.ProtocolClassifiers.Statistical
{
    public class FlowVector
    {

    }
    public class StatisticalClassifier : IProtocolClassifier<FlowVector>
    {
        public void LoadConfiguration(string filepath)
        {
            throw new NotImplementedException();
        }

        public ClassifierMatch Match(Conversation<FlowVector> conversation)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClassifierMatch> Matches(Conversation<FlowVector> conversation)
        {
            throw new NotImplementedException();
        }

        public void StoreConfiguration(string filepath)
        {
            throw new NotImplementedException();
        }

        public void Train(string protocol, Conversation<FlowVector> conversation)
        {
            throw new NotImplementedException();
        }
    }
}
