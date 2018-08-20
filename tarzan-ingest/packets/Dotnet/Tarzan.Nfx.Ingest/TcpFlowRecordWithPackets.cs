using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tarzan.Nfx.Ingest
{
    public class TcpFlowRecordWithPackets : FlowRecordWithPackets
    {
        public TcpFlowRecordWithPackets(Guid flowId, long firstSeen, long lastSeen, long octets, int packets, System.Collections.Generic.List<(Packet, PosixTimeval)> list) : base(flowId, firstSeen, lastSeen, octets, packets, list)
        {
        }

        public long TcpInitialSequenceNumber { get; set; }
        public new static TcpFlowRecordWithPackets From((Packet, PosixTimeval) capture)
        {
            return new TcpFlowRecordWithPackets(Guid.NewGuid(), (long)capture.Item2.MicroSeconds, (long)capture.Item2.MicroSeconds, capture.Item1.BytesHighPerformance.BytesLength, 1, new List<(Packet, PosixTimeval)> { capture });
        }
        /// <summary>
        /// Merges two existing flow records. It takes uuid from the first record.
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static TcpFlowRecordWithPackets Merge(TcpFlowRecordWithPackets f1, FlowRecordWithPackets f2)
        {
            return new TcpFlowRecordWithPackets(
                f1.FlowId,
                Math.Min(f1.FirstSeen, f2.FirstSeen),
                Math.Max(f1.FirstSeen, f2.FirstSeen),
                f1.Octets + f2.Octets,
                f1.Packets + f2.Packets,
                f1.PacketList.Concat(f2.PacketList).ToList())
            { TcpInitialSequenceNumber = f1.TcpInitialSequenceNumber };
        }
    }
}
