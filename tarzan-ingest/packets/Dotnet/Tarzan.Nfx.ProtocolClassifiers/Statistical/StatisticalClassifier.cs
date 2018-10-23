using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Analyzers;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.ProtocolClassifiers.Statistical
{
    public class FlowStatisticalVector
    {

    }
    public class StatisticalClassifier : IProtocolClassifier<FlowRecord<FlowStatisticalVector>>
    {
        public void LoadConfiguration(string filepath)
        {
            throw new NotImplementedException();
        }

        public ClassifierMatch Match(Conversation<FlowRecord<FlowStatisticalVector>> conversation)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClassifierMatch> Matches(Conversation<FlowRecord<FlowStatisticalVector>> conversation)
        {
            throw new NotImplementedException();
        }

        public void StoreConfiguration(string filepath)
        {
            throw new NotImplementedException();
        }

        public void Train(string protocol, Conversation<FlowRecord<FlowStatisticalVector>> conversation)
        {
            throw new NotImplementedException();
        }
    }
}
