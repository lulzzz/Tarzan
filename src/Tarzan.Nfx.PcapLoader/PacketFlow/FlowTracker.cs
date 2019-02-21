using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.PcapLoader.PacketFlow
{
    public class PacketFlowTracker : IFlowTracker<IList<FrameData>>
    {
        private readonly IKeyProvider<FlowKey, FrameData> m_keyProvider;

        public PacketFlowTracker(IKeyProvider<FlowKey, FrameData> keyProvider)
        {
            m_keyProvider = keyProvider;
        }

        public int TotalFrameCount { get; private set; }

        public IDictionary<FlowKey, IList<FrameData>> FlowTable { get; } = new Dictionary<FlowKey, IList<FrameData>>();

        public void ProcessFrame(FrameData frame)
        {
            if (frame == null) return;
            TotalFrameCount++;
            var key = m_keyProvider.GetKey(frame);
            if (FlowTable.TryGetValue(key, out var flowObject))
            {
                flowObject.Add(frame);
            }
            else
            {
                var flowUid = string.Empty;
                FlowTable[key] = new List<FrameData> { frame };
            }
        }

        public void ProcessFrames(IEnumerable<FrameData> frames)
        {
            foreach (var frame in frames)
            {
                ProcessFrame(frame);
            }
        }

        public void Reset()
        {
            FlowTable.Clear();
            TotalFrameCount = 0;
        }
    }
}
