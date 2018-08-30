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
    class FlowTracker
    {
        public FlowTracker()
        {
            FlowTable = new Dictionary<FlowKey, PacketStream>();
        }
        public Dictionary<FlowKey, PacketStream> FlowTable { get; private set; }
        public int TotalFrameCount { get; private set; }

        /// <summary>
        /// Captures all packets and tracks flows.
        /// </summary>
        public void ProcessFrame(Frame frame)
        {
            if (frame == null) return;
            TotalFrameCount++;
            var key = FlowKey.GetKey(frame.Data);
            if (FlowTable.TryGetValue(key, out var lst))
            {
                PacketStream.Update(lst, frame);
            }
            else
            {
                FlowTable[key] = PacketStream.From(frame);
            }
        }
    }
}
