using System.Collections.Generic;
using System.Security.Authentication;
using PacketDotNet;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public interface ITlsSessionBuilder
    {
        void SetFlowKey(FlowKey flowKey);
        void SetVersion(SslProtocols version);
        void SetFromClientHello(TlsPacket.TlsClientHello clientHello);
        void SetFromServerHello(TlsPacket.TlsServerHello serverHello);
        void SetServerCertificate(TlsPacket.TlsCertificate certificate);
        void SetApplicationData(TlsPacket.TlsApplicationData applicationData, TlsDirection direction, PacketMeta recordMeta, IEnumerable<(PacketMeta Meta, TcpPacket Packet)> tcpPackets);
    }
}