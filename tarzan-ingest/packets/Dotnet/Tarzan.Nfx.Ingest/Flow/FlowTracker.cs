using System.Collections.Generic;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Flow
{
    /// <summary>
    /// This class implementes a simple algorithm that groups packet into flows. 
    /// Flows are stored in the dictionary.
    /// </summary>
    class FlowTracker
    {
        /// <summary>
        /// Gets the dictionary of all existing flows.
        /// </summary>
        public Dictionary<FlowKey, (PacketFlow flow, PacketStream stream)> FlowTable { get; private set; }

        IKeyProvider<FlowKey, Frame> m_keyProvider;
        /// <summary>
        /// Gets the number of packets that were processed since this object was created.
        /// </summary>
        public int TotalFrameCount { get; private set; }

        public FlowTracker(IKeyProvider<FlowKey, Frame> keyProvider)
        {
            FlowTable = new Dictionary<FlowKey, (PacketFlow, PacketStream)>();
            m_keyProvider = keyProvider;
        }

        
        /// <summary>
        /// Processes the provided packet and creates or updates the corresponding flow.
        /// </summary>
        public void ProcessFrame(Frame frame)
        {
            if (frame == null) return;
            TotalFrameCount++;
            var key = m_keyProvider.GetKey(frame);
            if (FlowTable.TryGetValue(key, out var value))
            {
                PacketFlowFactory.Update(value.flow, frame);
                PacketStreamFactory.Update(value.stream, frame);
            }
            else
            {
                var flowUid = FlowUidGenerator.NewUid(key, frame.Timestamp).ToString();
                FlowTable[key] = (PacketFlowFactory.From(key,frame, flowUid), PacketStreamFactory.From(key, frame, flowUid));
            }
        }

        public void ProcessFrames(IEnumerable<Frame> frames)
        {
            foreach(var frame in frames)
            {
                ProcessFrame(frame);
            }
        }
    }
}
