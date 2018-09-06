using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using PacketDotNet;
using SharpPcap;

namespace Tarzan.Nfx.Ingest
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
        public Dictionary<FlowKey, PacketStream> FlowTable { get; private set; }

        IKeyProvider<FlowKey, Frame> m_keyProvider;
        /// <summary>
        /// Gets the number of packets that were processed since this object was created.
        /// </summary>
        public int TotalFrameCount { get; private set; }

        public FlowTracker(IKeyProvider<FlowKey, Frame> keyProvider)
        {
            FlowTable = new Dictionary<FlowKey, PacketStream>();
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
                PacketStream.Update(value, frame);
            }
            else
            {
                FlowTable[key] = PacketStream.From(key, frame);
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
