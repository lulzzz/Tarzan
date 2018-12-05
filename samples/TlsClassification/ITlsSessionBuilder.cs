using System.Collections.Generic;
using System.Security.Authentication;
using PacketDotNet;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Samples.TlsClassification
{

    public struct TlsPacketContext
    {
        public TlsDirection Direction;
        public PacketMeta Metadata;
        public IEnumerable<(PacketMeta Meta, TcpPacket Packet)> TcpPackets;

        public override string ToString()
        {
            return $"[TlsPacketContext direction={Direction}, meta={Metadata}]";
        }
    }
    public interface ITlsSessionBuilder
    {
        void SetFlowKey(FlowKey flowKey);
        void SetClientHello(TlsPacket.TlsClientHello clientHello, TlsPacketContext packetContext);
        void SetServerHello(TlsPacket.TlsServerHello serverHello, TlsPacketContext packetContext);
        void SetServerCertificate(TlsPacket.TlsCertificate certificate, TlsPacketContext packetContext);
        void AddApplicationDataRecord(TlsPacket.TlsApplicationData applicationData, TlsPacketContext packetContext);
    }
}