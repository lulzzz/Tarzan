using System;
using System.Collections.Generic;
using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;
using System.Linq;

namespace Tarzan.Nfx.Ingest
{
    /// <summary>
    /// Extends <see cref="FlowRecord"/> with other properties, such as, <see cref="FlowId"/>, <see cref="ServiceName"/>, and mainly
    /// <see cref="PacketList"/>.
    /// </summary>
    public class PacketStream : FlowRecord
    {
        public IList<(Packet packet, PosixTimeval timeval)> PacketList { get; private set; }

        public Guid FlowId { get; private set; }

        public string ServiceName { get; set; }

        protected PacketStream(Guid flowId, long firstSeen, long lastSeen, long octets, int packets, List<(Packet, PosixTimeval)> list)
        {
            FlowId = flowId;
            FirstSeen = firstSeen;
            LastSeen = lastSeen;
            Octets = octets;
            Packets = packets;
            PacketList = list;
        }

        public static PacketStream From((Packet packet, PosixTimeval timeval) capture)
        {
            return new PacketStream(Guid.NewGuid(), (long)capture.timeval.MicroSeconds, (long)capture.timeval.MicroSeconds, capture.packet.BytesHighPerformance.BytesLength, 1, new List<(Packet, PosixTimeval)> { capture });
        }                     
        /// <summary>
        /// Merges two existing flow records. It takes uuid from the first record.
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static PacketStream Merge(PacketStream f1, PacketStream f2)
        {
            return new PacketStream(
                f1.FlowId,
                Math.Min(f1.FirstSeen, f2.FirstSeen),
                Math.Max(f1.FirstSeen, f2.FirstSeen),
                f1.Octets + f2.Octets,
                f1.Packets + f2.Packets,
                f1.PacketList.Concat(f2.PacketList).ToList());
        }

        public bool IntersectsWith(PacketStream that)
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
