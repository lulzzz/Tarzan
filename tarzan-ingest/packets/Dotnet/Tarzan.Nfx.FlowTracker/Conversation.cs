﻿using System.Collections.Generic;

namespace Tarzan.Nfx.FlowTracker
{
    public struct Conversation<TFlowValue>
    {
        public FlowKey ConversationKey;
        public TFlowValue Upflow;
        public TFlowValue Downflow;

        public Conversation(KeyValuePair<FlowKey, TFlowValue> x, KeyValuePair<FlowKey, TFlowValue> y) : this()
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