using Apache.Ignite.Core.Binary;
using PacketDotNet;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Tcp
{
    /// <summary>
    /// The space-time model (inspired by Lamport) represents a Tcp communication between two endpoints.
    /// </summary>
    /// <remarks>
    /// Each message represents an event. The event has assigned the time as the offset of the message occurence form the 
    /// start of the conversations. The space axis shows the size of the message. The size is positive if the message is from 
    /// client to the server or negative for the other direction. 
    /// </remarks>
    [Serializable]
    public class TcpSpaceTimeModel : IBinarizable
    {
        public FlowKey FlowKey { get; set; }

        public long TimeOrigin { get; set; }

        public Event [] Events { get; set; }

        public void ReadBinary(IBinaryReader reader)
        {
            TimeOrigin = reader.ReadLong(nameof(TimeOrigin));
            Events = reader.ReadArray<Event>(nameof(Events));
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteLong(nameof(TimeOrigin), TimeOrigin);
            writer.WriteArray(nameof(Events), Events);
        }

        /// <summary>
        /// Denotes a single event in the space-time model. The time is captured by <see cref="Offset"/> property.
        /// The space is given in term of message <see cref="Size"/>. The event is labeled by <see cref="TcpFlags"/>.
        /// </summary>
        public class Event : IBinarizable
        {
            public long Offset { get; set; }
            public int Size { get; set; }
            public ushort TcpFlags { get; set; }

            public bool Urg => (TcpFlags & PacketDotNet.TcpFields.TCPUrgMask) != 0; 
            public bool Ack => (TcpFlags & PacketDotNet.TcpFields.TCPAckMask) != 0;
            public bool Psh => (TcpFlags & PacketDotNet.TcpFields.TCPPshMask) != 0;
            public bool Rst => (TcpFlags & PacketDotNet.TcpFields.TCPRstMask) != 0;
            public bool Syn => (TcpFlags & PacketDotNet.TcpFields.TCPSynMask) != 0;
            public bool ECN => (TcpFlags & PacketDotNet.TcpFields.TCPEcnMask) != 0;
            public bool CWR => (TcpFlags & PacketDotNet.TcpFields.TCPCwrMask) != 0;
            public bool Fin => (TcpFlags & PacketDotNet.TcpFields.TCPFinMask) != 0;

            public void SetFlag(bool on, int MASK)
            {
                if (on)
                    TcpFlags = (byte)(TcpFlags | MASK);
                else
                    TcpFlags = (byte)(TcpFlags & ~MASK);
            }

            public void ReadBinary(IBinaryReader reader)
            {
                Offset = reader.ReadLong(nameof(Offset));
                Size = reader.ReadInt(nameof(Size));
                TcpFlags = (ushort)reader.ReadInt(nameof(TcpFlags));
            }

            public void WriteBinary(IBinaryWriter writer)
            {
                writer.WriteLong(nameof(Offset), Offset);
                writer.WriteInt(nameof(Size), Size);
                writer.WriteInt(nameof(TcpFields), TcpFlags);
            }
        }
    }
}
