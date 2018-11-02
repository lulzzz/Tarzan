using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Utils
{
    /// <summary>
    /// This class implementes a simple algorithm that groups packet into flows. 
    /// Flows are stored in the local dictionary.
    /// </summary>
    /// <remarks>
    /// As a further optimization we may consider the method used by DPDK:
    /// https://dpdk.readthedocs.io/en/v16.04/prog_guide/hash_lib.html#hash-api-overview.
    /// </remarks>
    public partial class FlowTracker : IFlowTracker<FlowData>
    {
        /// <summary>
        /// Gets the dictionary of all existing flows.
        /// </summary>
        public Dictionary<FlowKey, FlowData> FlowTable { get; private set; }

        IKeyProvider<FlowKey, FrameData> m_keyProvider;
        /// <summary>
        /// Gets the number of packets that were processed since this object was created.
        /// </summary>
        public int TotalFrameCount { get; private set; }

        IDictionary<FlowKey, FlowData> IFlowTracker<FlowData>.FlowTable => FlowTable;

        public FlowTracker(IKeyProvider<FlowKey, FrameData> keyProvider)
        {
            FlowTable = new Dictionary<FlowKey, FlowData>();
            m_keyProvider = keyProvider;
        }
        
        /// <summary>
        /// Processes the provided packet and creates or updates the corresponding flow.
        /// </summary>
        public void ProcessFrame(FrameData frame)
        {
            if (frame == null) return;
            TotalFrameCount++;
            var key = m_keyProvider.GetKey(frame);
            if (FlowTable.TryGetValue(key, out var value))
            {
                PacketFlowUpdate(value, frame);
            }
            else
            {
                var flowUid = string.Empty; 
                FlowTable[key] = PacketFlowFrom(key, frame, flowUid);
            }
        }

        private FlowData PacketFlowFrom(FlowKey flowKey, FrameData frame, string flowUid)
        {
            return new FlowData()
            {
                FlowUid = flowUid,
                Protocol = flowKey.Protocol.ToString(),
                SourceAddress = flowKey.SourceIpAddress.ToString(),
                SourcePort = flowKey.SourcePort,
                DestinationAddress = flowKey.DestinationIpAddress.ToString(),
                DestinationPort = flowKey.DestinationPort,
                FirstSeen = frame.Timestamp,
                LastSeen = frame.Timestamp,
                Octets = frame.Data.Length,
                Packets = 1
            };
        }
        private FlowData PacketFlowUpdate(FlowData packetFlow, FrameData frame)
        {
            packetFlow.FirstSeen = Math.Min(packetFlow.FirstSeen, frame.Timestamp);
            packetFlow.LastSeen = Math.Max(packetFlow.LastSeen, frame.Timestamp);
            packetFlow.Octets += frame.Data.Length;
            packetFlow.Packets++;
            return packetFlow;
        }

        public void ProcessFrames(IEnumerable<FrameData> frames)
        {
            foreach(var frame in frames)
            {
                ProcessFrame(frame);
            }
        }
    }
}
