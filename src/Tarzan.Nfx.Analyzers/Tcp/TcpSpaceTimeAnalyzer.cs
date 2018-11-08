using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Tcp
{


    
    [Serializable]
    public class TcpSpaceTime : IBinarizable
    {
        public FlowKey FlowKey { get; set; }

        public long TimeOrigin { get; set; }

        public TcpPacketVector [] Vectors { get; set; }

        public void ReadBinary(IBinaryReader reader)
        {
            TimeOrigin = reader.ReadLong(nameof(TimeOrigin));
            Vectors = reader.ReadArray<TcpPacketVector>(nameof(Vectors));
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteLong(nameof(TimeOrigin), TimeOrigin);
            writer.WriteArray(nameof(Vectors), Vectors.ToArray());
        }

        public class TcpPacketVector : IBinarizable
        {
            public long Offset { get; set; }
            public int Size { get; set; }
            public byte TcpFlags { get; set; }

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
                TcpFlags = (byte)reader.ReadByte(nameof(TcpFlags));
            }

            public void WriteBinary(IBinaryWriter writer)
            {
                writer.WriteLong(nameof(Offset), Offset);
                writer.WriteInt(nameof(Size), Size);
                writer.WriteByte(nameof(TcpFields), TcpFlags);
            }
        }
    }

    public class TcpSpaceTimeAnalyzer : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;



        public void Invoke()
        {
            throw new NotImplementedException();
        }
    }
}
