using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using PacketDotNet;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using Tarzan.Nfx.Tls;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    class TlsDecoderBuilder : ITlsSessionBuilder
    {
        TlsDecoder m_tlsDecoder;
        public TlsDecoderBuilder()
        {
            m_tlsDecoder = new TlsDecoder();
        }

        public void AddApplicationDataRecord(TlsPacket.TlsApplicationData applicationData,TlsPacketContext packetContext)
        { }

        public void SetClientHello(TlsPacket.TlsClientHello clientHello, TlsPacketContext packetContext)
        {
            m_tlsDecoder.ClientRandom = ByteString.Combine(clientHello.Random.RandomTime, clientHello.Random.RandomBytes);
        }

        public void SetFlowKey(FlowKey flowKey)
        {
        }

        public void SetServerCertificate(TlsPacket.TlsCertificate certificate, TlsPacketContext packetContext)
        {
        }

        public void SetServerHello(TlsPacket.TlsServerHello serverHello, TlsPacketContext packetContext)
        {
            m_tlsDecoder.ProtocolVersion = TlsSecurityParameters.GetSslProtocolVersion(serverHello.Version.Major, serverHello.Version.Minor);
            m_tlsDecoder.ServerRandom = ByteString.Combine(serverHello.Random.RandomTime, serverHello.Random.RandomBytes);
            m_tlsDecoder.CipherSuite = (TlsCipherSuite)serverHello.CipherSuite.CipherId;
            m_tlsDecoder.Compression = serverHello.CompressionMethod;
        }

        public TlsDecoder ToDecoder()
        {
            return this.m_tlsDecoder;
        }
    }
}
