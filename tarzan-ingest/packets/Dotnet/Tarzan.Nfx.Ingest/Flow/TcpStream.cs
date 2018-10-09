using Netdx.PacketDecoders;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Ingest.Utils;
namespace Tarzan.Nfx.Ingest.Flow
{
    /// <summary>
    /// Represents TCP stream of packets. 
    /// </summary>
    public class TcpStream
    {
        public long TcpInitialSequenceNumber { get; set; }

        public IList<(TcpPacket Packet, PosixTime Timeval)> SegmentList { get; private set; }

        public string ServiceName { get; set; }
        public long FirstSeen { get; private set; }
        public long LastSeen { get; private set; }
        public long Octets { get; private set; }
        public int Packets { get; private set; }

        public TcpStream(long firstSeen, long lastSeen, long octets, int packets, List<(TcpPacket, PosixTime)> list)
        {
            FirstSeen = firstSeen;
            LastSeen = lastSeen;
            Octets = octets;
            Packets = packets;
            SegmentList = list;
        }
       
        public static TcpStream From((TcpPacket packet, PosixTime timeval) capture)
        {
            var unixtime = capture.timeval.ToUnixTimeMilliseconds();
            return new TcpStream(unixtime, unixtime, capture.packet.BytesHighPerformance.BytesLength, 1, new List<(TcpPacket, PosixTime)> { capture })
            {
                TcpInitialSequenceNumber = capture.packet.SequenceNumber
            };
        }
        /// <summary>
        /// Merges two existing flow records. It takes uuid from the first record.
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static TcpStream Merge(TcpStream f1, TcpStream f2)
        {
            return new TcpStream(
                Math.Min(f1.FirstSeen, f2.FirstSeen),
                Math.Max(f1.FirstSeen, f2.FirstSeen),
                f1.Octets + f2.Octets,
                f1.Packets + f2.Packets,
                f1.SegmentList.Concat(f2.SegmentList).ToList())
            { TcpInitialSequenceNumber = f1.TcpInitialSequenceNumber };
        }

        /// <summary>
        /// Pairs tcp streams in the provided collection of flows.
        /// </summary>
        /// <param name="flows">Enumerable collection of source flows to find matching TCP streams.</param>
        /// <returns>A collection of TCP conversations.</returns>
        public static IEnumerable<TcpConversation> Pair(IEnumerable<KeyValuePair<FlowKey, TcpStream>> flows)
        {
            var downFlows = flows.Where(f => f.Key.DestinationPort < f.Key.SourcePort);
            var upFlows = flows.Where(f => f.Key.DestinationPort > f.Key.SourcePort);
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
        public static IEnumerable<KeyValuePair<FlowKey, TcpStream>> Split(IEnumerable<KeyValuePair<FlowKey, PacketStream>> flows)
        {
            foreach (var (flowKey, flowRecord) in flows.Where(f => f.Key.Protocol == ProtocolType.Tcp))
            {
                TcpStream currentFlowRecord = null;
                // search for SYN, FIN or RST
                // SYN means to create a new flow 
                // FIN and RST means to finish the current flow
                foreach (var frame in flowRecord.FrameList)
                {
                    var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
                    var tcp = packet.Extract(typeof(TcpPacket)) as TcpPacket;
                    if (tcp.Syn || currentFlowRecord == null)
                    {
                        if (currentFlowRecord != null) yield return KeyValuePair.Create(flowKey, currentFlowRecord);
                        currentFlowRecord = From((tcp, PosixTime.FromUnixTimeMilliseconds(frame.Timestamp)));
                        currentFlowRecord.TcpInitialSequenceNumber = tcp.SequenceNumber;
                    }
                    else
                    {
                        currentFlowRecord = Merge(currentFlowRecord, TcpStream.From((tcp, PosixTime.FromUnixTimeMilliseconds(frame.Timestamp))));
                    }
                }
                if (currentFlowRecord != null) yield return KeyValuePair.Create(flowKey, currentFlowRecord);
            }
        }

        public static IEnumerable<(PosixTime Timeval, bool Syn, bool Fin, bool Rst, bool Ack, bool Psh, long SequenceNumber, long AcknowledgmentNumber, int Length)> SegmentMap(TcpStream stream)
        {
            foreach (var (packet, timeval) in stream.SegmentList)
            {
                yield return (timeval, packet.Syn, packet.Fin, packet.Rst, packet.Ack, packet.Psh, packet.SequenceNumber, packet.AcknowledgmentNumber, packet.PayloadData?.Length ?? 0);
            }
        }

        public bool IntersectsWith(TcpStream that)
        {
            if (this.FirstSeen <= that.FirstSeen)
            {
                return that.FirstSeen <= this.LastSeen;
            }
            else
            {
                return this.FirstSeen <= that.LastSeen;
            }
        }

    }
}
