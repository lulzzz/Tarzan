using System;
using System.Collections.Generic;
using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;
using System.Linq;

namespace Tarzan.Nfx.Ingest
{
    public class FlowRecordWithPackets : FlowRecord
    {
        IList<(Packet packet, PosixTimeval time)> m_packetList;

        protected FlowRecordWithPackets(Guid flowId, long firstSeen, long lastSeen, long octets, int packets, List<(Packet, PosixTimeval)> list)
        {
            FlowId = flowId;
            FirstSeen = firstSeen;
            LastSeen = lastSeen;
            Octets = octets;
            Packets = packets;
            m_packetList = list;
        }

      

        public static FlowRecordWithPackets From((Packet, PosixTimeval) capture)
        {
            return new FlowRecordWithPackets(Guid.NewGuid(), (long)capture.Item2.MicroSeconds, (long)capture.Item2.MicroSeconds, capture.Item1.BytesHighPerformance.BytesLength, 1, new List<(Packet, PosixTimeval)> { capture });
        }                     
        /// <summary>
        /// Merges two existing flow records. It takes uuid from the first record.
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static FlowRecordWithPackets Merge(FlowRecordWithPackets f1, FlowRecordWithPackets f2)
        {
            return new FlowRecordWithPackets(
                f1.FlowId,
                Math.Min(f1.FirstSeen, f2.FirstSeen),
                Math.Max(f1.FirstSeen, f2.FirstSeen),
                f1.Octets + f2.Octets,
                f1.Packets + f2.Packets,
                f1.PacketList.Concat(f2.PacketList).ToList());
        }

        public IList<(Packet packet, PosixTimeval time)> PacketList => m_packetList;

        public Guid FlowId { get; private set; }

        public string ServiceName { get; internal set; }

        public bool IntersectsWith(FlowRecordWithPackets that)
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
