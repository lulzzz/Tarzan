using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PacketDotNet;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public class TlsFlowModelBuilder : ITlsSessionBuilder
    {
        TlsFlowModel m_flowData = new TlsFlowModel();

        public TlsFlowModel FlowData { get => m_flowData; }

        public void SetFlowKey(FlowKey flowKey)
        {
            m_flowData.Key = flowKey;
        }

        public void SetFromClientHello(TlsPacket.TlsClientHello clientHello)
        {
            m_flowData.SessionId = ByteString.ByteArrayToString(clientHello.SessionId.Sid);
            m_flowData.ClientRandom = ByteString.ByteArrayToString(clientHello.Random.RandomBytes);
            m_flowData.ClientCipherSuites = GetCiphersStringArray(clientHello.CipherSuites);
            m_flowData.ClientExtensions = GetExtensionStringArray(clientHello.Extensions);
        }

        public void SetFromServerHello(TlsPacket.TlsServerHello serverHello)
        {
            m_flowData.SessionId = ByteString.ByteArrayToString(serverHello.SessionId.Sid);
            m_flowData.ServerRandom = ByteString.ByteArrayToString(serverHello.Random.RandomBytes);
            m_flowData.ServerCipherSuite = $"{(TlsCipherSuite)serverHello.CipherSuite.CipherId}";
            m_flowData.ServerExtensions = GetExtensionStringArray(serverHello.Extensions);
        }

        public void SetVersion(SslProtocols version)
        {
            m_flowData.Version = version.ToString();
        }

        public void SetServerCertificate(TlsPacket.TlsCertificate certificate)
        {
            var x509Certificates = certificate.Certificates.Select(x => new X509Certificate2(x.Body));
            m_flowData.ServerCertificates = x509Certificates.Select(x => x.SubjectName.Name).ToArray();
        }

        public void SetApplicationData(TlsPacket.TlsApplicationData applicationData, TlsDirection direction, PacketMeta recordMeta, IEnumerable<(PacketMeta Meta, TcpPacket Packet)> tcpPackets)
        {
            var segments = tcpPackets.Select(packet => new TcpSegmentModel
            {
                TimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(packet.Meta.Timestamp) - m_flowData.Timestamp,
                PacketNumber = packet.Meta.Number,
                Flags = TcpFlags(packet.Packet),
                Length = packet.Packet.PayloadData?.Length ?? 0,
                Window = packet.Packet.WindowSize
            }
            );

            var record = new TlsRecordModel
            {
                RecordNumber = recordMeta.Number,
                Direction = direction,
                TimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(recordMeta.Timestamp) - m_flowData.Timestamp,
                Length = applicationData.Body.Length,
                Segments = new List<TcpSegmentModel>(segments),
            };

            m_flowData.Records.Add(record);
        }

        private static string[] GetCiphersStringArray(TlsPacket.CipherSuites cipherSuites)
        {
            return cipherSuites.Items.Select(x => ((TlsCipherSuite)x).ToString()).ToArray();
        }
        private static string[] GetExtensionStringArray(TlsPacket.TlsExtensions extensions)
        {
            return new string[] { };
        }

        private static string TcpFlags(TcpPacket packet)
        {
            var flags = new List<string>();
            if (packet.Syn) flags.Add("SYN");
            if (packet.Ack) flags.Add("ACK");
            if (packet.Psh) flags.Add("PSH");
            if (packet.Rst) flags.Add("RST");
            if (packet.Fin) flags.Add("FIN");
            return String.Join(',', flags);
        }
    }

}
