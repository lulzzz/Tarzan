using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.FlowTracker
{

    /// <summary>
    /// This class implementes a simple algorithm that groups packet into flows. 
    /// Flows are stored in the dictionary.
    /// </summary>
    /// <remarks>
    /// As a further optimization we may consider the method used by DPDK:
    /// https://dpdk.readthedocs.io/en/v16.04/prog_guide/hash_lib.html#hash-api-overview.
    /// </remarks>
    public partial class FlowWithContentTracker : IFlowTracker<FlowRecord>
    {
        /// <summary>
        /// Gets the dictionary of all existing flows.
        /// </summary>
        public Dictionary<FlowKey, FlowRecord> FlowTable { get; private set; }

        IKeyProvider<FlowKey, Frame> m_keyProvider;
        /// <summary>
        /// Gets the number of packets that were processed since this object was created.
        /// </summary>
        public int TotalFrameCount { get; private set; }

        IDictionary<FlowKey, FlowRecord> IFlowTracker<FlowRecord>.FlowTable => FlowTable;

        public FlowWithContentTracker(IKeyProvider<FlowKey, Frame> keyProvider)
        {
            FlowTable = new Dictionary<FlowKey, FlowRecord>();
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
                PacketFlowUpdate(value.Flow, frame);
                PacketStreamUpdate(value.Stream, frame);
            }
            else
            {
                var flowUid = string.Empty; // FlowUidGenerator.NewUid(key, frame.Timestamp).ToString();
                FlowTable[key] = new FlowRecord { Flow = PacketFlowFrom(key, frame, flowUid), Stream = PacketStreamFrom(key, frame, flowUid) };
            }
        }

        private PacketStream PacketStreamFrom(FlowKey key, Frame frame, string flowUid)
        {
            return new PacketStream()
            {
                FlowUid = flowUid,
                FrameList = new List<Frame> { frame }
            };
        }

        private PacketFlow PacketFlowFrom(FlowKey flowKey, Frame frame, string flowUid)
        {
            return new PacketFlow()
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

        private PacketStream PacketStreamUpdate(PacketStream packetStream, Frame frame)
        {
            packetStream.FrameList.Add(frame);
            return packetStream;
        }

        private PacketFlow PacketFlowUpdate(PacketFlow packetFlow, Frame frame)
        {
            packetFlow.FirstSeen = Math.Min(packetFlow.FirstSeen, frame.Timestamp);
            packetFlow.LastSeen = Math.Max(packetFlow.LastSeen, frame.Timestamp);
            packetFlow.Octets += frame.Data.Length;
            packetFlow.Packets++;
            return packetFlow;
        }

        public void ProcessFrames(IEnumerable<Frame> frames)
        {
            foreach(var frame in frames)
            {
                ProcessFrame(frame);
            }
        }

        /// <summary>
        /// Gets the conversation for the given flow key. If neither upflow or
        /// down flow exists in the flow table it returns <c>null</c>.
        /// </summary>
        /// <param name="upflowKey"></param>
        /// <returns></returns>
        public Conversation<FlowRecord>? GetConversation(FlowKey upflowKey)
        {
            var upflowExists = FlowTable.TryGetValue(upflowKey, out var upflow);
            var downFlowKey = upflowKey.SwapEndpoints();
            var downflowExists = FlowTable.TryGetValue(downFlowKey, out var downflow);
            if (upflowExists || downflowExists)
                return new Conversation<FlowRecord>() { ConversationKey = upflowKey, Upflow = upflow, Downflow = downflow };
            else return null;
        }
    }
}
