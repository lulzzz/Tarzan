using Cassandra.Data.Linq;
using Netdx.ConversationTracker;
using PacketDotNet;
using System.Collections.Generic;
using System.Linq;

namespace Tarzan.Nfx.Ingest
{
        static class TcpFlows
        {
            public static IEnumerable<Conversation> Pair(IEnumerable<KeyValuePair<FlowKey, TcpFlowRecordWithPackets>> flows)
            {
                var downFlows = flows.Where(f => f.Key.DestinationEndpoint.Port < f.Key.SourceEndpoint.Port);
                var upFlows = flows.Where(f => f.Key.DestinationEndpoint.Port > f.Key.SourceEndpoint.Port);
                var conversationCandidates = downFlows.Join(upFlows, f => f.Key, f => f.Key, (f1, f2) => new Conversation { RequestFlow = f1, ResponseFlow = f2 }, new Conversation.Comparer());
                return conversationCandidates.Where(c => c.RequestFlow.Value.IntersectsWith(c.ResponseFlow.Value));
            }

            /// <summary>
            /// Analyze a sequence of flows and returns a new sequence of Tcp flows. This method split Packet Flow into Tcp Streams.
            /// </summary>
            /// <param name="flows"></param>
            /// <returns></returns>
            public static IEnumerable<KeyValuePair<FlowKey, TcpFlowRecordWithPackets>> Isolate(IEnumerable<KeyValuePair<FlowKey, FlowRecordWithPackets>> flows)
            {
                foreach (var (flowKey, flowRecord) in flows.Where(f => f.Key.Protocol == ProtocolType.TCP))
                {
                    TcpFlowRecordWithPackets currentFlowRecord = null;
                    // search for SYN, FIN or RST
                    // SYN means to create a new flow 
                    // FIN and RST means to finish the current flow
                    foreach (var packet in flowRecord.PacketList)
                    {
                        var tcp = packet.packet.Extract(typeof(TcpPacket)) as TcpPacket;
                        if (tcp.Syn || currentFlowRecord == null)
                        {
                            if (currentFlowRecord != null) yield return KeyValuePair.Create(flowKey, currentFlowRecord);
                            currentFlowRecord = TcpFlowRecordWithPackets.From(packet);
                            currentFlowRecord.TcpInitialSequenceNumber = tcp.SequenceNumber;
                        }
                        else
                        {
                            currentFlowRecord = TcpFlowRecordWithPackets.Merge(currentFlowRecord, TcpFlowRecordWithPackets.From(packet));
                        }
                    }
                    if (currentFlowRecord != null) yield return KeyValuePair.Create(flowKey, currentFlowRecord);
                }
            }


        }
}
