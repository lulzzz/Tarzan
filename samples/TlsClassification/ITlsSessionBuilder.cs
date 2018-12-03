using System.Collections.Generic;
using System.Security.Authentication;
using PacketDotNet;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Samples.TlsClassification
{

    public struct TlsPacketContext
    {
        public TlsDirection direction;
        public PacketMeta recordMeta;
        public IEnumerable<(PacketMeta Meta, TcpPacket Packet)> tcpPackets;
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