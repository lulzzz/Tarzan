using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Model
{
    public class Conversation<TFlowValue>
    {
        public FlowKey ConversationKey { get; set; }
        public TFlowValue Upflow { get; set; }
        public TFlowValue Downflow { get; set; }

        public Conversation() { }
        public Conversation(KeyValuePair<FlowKey, TFlowValue> x, KeyValuePair<FlowKey, TFlowValue> y)
        {
            if (x.Key.SourcePort > y.Key.SourcePort)
            {
                ConversationKey = x.Key;
                Upflow = x.Value;
                Downflow = y.Value;
            }
            else
            {
                ConversationKey = y.Key;
                Upflow = y.Value;
                Downflow = x.Value;
            }
        }
    }
}