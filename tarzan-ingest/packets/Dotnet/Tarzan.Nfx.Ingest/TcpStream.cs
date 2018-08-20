using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tarzan.Nfx.Ingest
{
    public class TcpStream : FlowPackets
    {
        public long TcpInitialSequenceNumber { get; set; }

        public TcpStream(Guid flowId, long firstSeen, long lastSeen, long octets, int packets, System.Collections.Generic.List<(Packet, PosixTimeval)> list) : base(flowId, firstSeen, lastSeen, octets, packets, list)
        {
        }
       
        public static TcpStream From((TcpPacket tcp, PosixTimeval timeval) capture)
        {
            return new TcpStream(Guid.NewGuid(), (long)capture.timeval.MicroSeconds, (long)capture.timeval.MicroSeconds, capture.tcp.BytesHighPerformance.BytesLength, 1, new List<(Packet, PosixTimeval)> { capture })
            {
                TcpInitialSequenceNumber = capture.tcp.SequenceNumber
            };
        }
        /// <summary>
        /// Merges two existing flow records. It takes uuid from the first record.
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static TcpStream Merge(TcpStream f1, FlowPackets f2)
        {
            return new TcpStream(
                f1.FlowId,
                Math.Min(f1.FirstSeen, f2.FirstSeen),
                Math.Max(f1.FirstSeen, f2.FirstSeen),
                f1.Octets + f2.Octets,
                f1.Packets + f2.Packets,
                f1.PacketList.Concat(f2.PacketList).ToList())
            { TcpInitialSequenceNumber = f1.TcpInitialSequenceNumber };
        }

        public static IEnumerable<TcpConversation> Pair(IEnumerable<KeyValuePair<FlowKey, TcpStream>> flows)
        {
            var downFlows = flows.Where(f => f.Key.DestinationEndpoint.Port < f.Key.SourceEndpoint.Port);
            var upFlows = flows.Where(f => f.Key.DestinationEndpoint.Port > f.Key.SourceEndpoint.Port);
            var conversationCandidates = downFlows.Join(upFlows, f => f.Key, f => f.Key, (f1, f2) => new TcpConversation { RequestFlow = f1, ResponseFlow = f2 }, new TcpConversation.Comparer());
            return conversationCandidates.Where(c => c.RequestFlow.Value.IntersectsWith(c.ResponseFlow.Value));
        }

        /// <summary>
        /// Analyze a sequence of flows and returns a new sequence of Tcp flows. This method splits Packet Flow into Tcp Streams.
        /// Usually there is 1-to-1 mapping, but in general the flow can be reused by a number of TCP streams.
        /// </summary>
        /// <param name="flows"></param>
        /// <remarks>
        /// The current implementation uses a simple rule to split a flow into streams. It searches for SYN flag that identifies 
        /// the new TCP stream. 
        /// </remarks>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<FlowKey, TcpStream>> Split(IEnumerable<KeyValuePair<FlowKey, FlowPackets>> flows)
        {
            foreach (var (flowKey, flowRecord) in flows.Where(f => f.Key.Protocol == ProtocolType.TCP))
            {
                TcpStream currentFlowRecord = null;
                // search for SYN, FIN or RST
                // SYN means to create a new flow 
                // FIN and RST means to finish the current flow
                foreach (var (packet, timeval) in flowRecord.PacketList)
                {
                    var tcp = packet.Extract(typeof(TcpPacket)) as TcpPacket;
                    if (tcp.Syn || currentFlowRecord == null)
                    {
                        if (currentFlowRecord != null) yield return KeyValuePair.Create(flowKey, currentFlowRecord);
                        currentFlowRecord = From((tcp, timeval));
                        currentFlowRecord.TcpInitialSequenceNumber = tcp.SequenceNumber;
                    }
                    else
                    {
                        currentFlowRecord = Merge(currentFlowRecord, TcpStream.From((packet, timeval)));
                    }
                }
                if (currentFlowRecord != null) yield return KeyValuePair.Create(flowKey, currentFlowRecord);
            }
        }

    }
}
