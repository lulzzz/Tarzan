using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.FlowTracker
{
    public interface IFlowTracker<TFlowValue>
    {
        IDictionary<FlowKey, TFlowValue> FlowTable { get; }
        int TotalFrameCount { get; }

        void ProcessFrame(Frame frame);
        void ProcessFrames(IEnumerable<Frame> frames);

        Conversation<TFlowValue>? GetConversation(FlowKey flowKey);
    }
}