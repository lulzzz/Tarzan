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
        TlsConversationModel m_conversationModel;

        public TlsConversationModel FlowData { get => m_conversationModel; }

        public TlsFlowModelBuilder(TlsConversationContext dbContext)
        {        
            m_conversationModel = new TlsConversationModel();
            m_conversationModel.Records = new List<TlsRecordModel>();
            dbContext.Add(m_conversationModel);
        }

        public void SetFlowKey(FlowKey flowKey)
        {
            m_conversationModel.ConversationKey = flowKey.ToString();
        }

        public void SetFromClientHello(TlsPacket.TlsClientHello clientHello)
        {
            m_conversationModel.SessionId = ByteString.ByteArrayToString(clientHello.SessionId.Sid);
            m_conversationModel.ClientRandom = ByteString.ByteArrayToString(clientHello.Random.RandomBytes);
            m_conversationModel.ClientCipherSuites = GetCipherSuites(clientHello.CipherSuites);
            m_conversationModel.ClientExtensions = GetExtensions(clientHello.Extensions);
        }

        public void SetFromServerHello(TlsPacket.TlsServerHello serverHello)
        {
            m_conversationModel.SessionId = ByteString.ByteArrayToString(serverHello.SessionId.Sid);
            m_conversationModel.ServerRandom = ByteString.ByteArrayToString(serverHello.Random.RandomBytes);
            m_conversationModel.ServerCipherSuite = $"{(TlsCipherSuite)serverHello.CipherSuite.CipherId}";
            m_conversationModel.ServerExtensions = GetExtensions(serverHello.Extensions);
        }

        public void SetVersion(SslProtocols version)
        {
            m_conversationModel.Version = version.ToString();
        }

        public void SetServerCertificate(TlsPacket.TlsCertificate certificate)
        {
            TlsCertificateModel CreateCertificate(X509Certificate2 cert)
            {
                return new TlsCertificateModel
                {
                    SubjectName = cert.SubjectName.Name,
                    IssuerName = cert.IssuerName.Name,
                    NotBefore = cert.NotBefore,
                    NotAfter = cert.NotAfter
                };
            }

            var x509Certificates = certificate.Certificates.Select(x => new X509Certificate2(x.Body));
            m_conversationModel.ServerCertificates = x509Certificates.Select(CreateCertificate).ToList();
        }

        public void SetApplicationData(TlsPacket.TlsApplicationData applicationData, TlsDirection direction, PacketMeta recordMeta, IEnumerable<(PacketMeta Meta, TcpPacket Packet)> tcpPackets)
        {
            var segments = tcpPackets.Select(packet => new TcpSegmentModel
            {
                TimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(packet.Meta.Timestamp) - m_conversationModel.Timestamp,
                PacketNumber = packet.Meta.Number,
                Flags = TcpFlags(packet.Packet),
                Length = packet.Packet.PayloadData?.Length ?? 0,
                Window = packet.Packet.WindowSize
            }
            );

            var record = new TlsRecordModel
            {
                ConversationKey = m_conversationModel.ConversationKey,
                RecordNumber = recordMeta.Number,
                Direction = direction,
                TimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(recordMeta.Timestamp) - m_conversationModel.Timestamp,
                Length = applicationData.Body.Length,
                Segments = new List<TcpSegmentModel>(segments),
            };

            m_conversationModel.Records.Add(record);
        }

        private static string GetCipherSuites(TlsPacket.CipherSuites cipherSuites)
        {
            var suites = cipherSuites.Items.Select(x => ((TlsCipherSuite)x).ToString());
            return $"[{String.Join(',', suites)}]";
        }
        private static string GetExtensions(TlsPacket.TlsExtensions extensions)
        {
            IEnumerable<(string Name,string Value)> CollectionExtensions()
            {
                foreach (var item in extensions.Items)
                {
                    switch (item.Body)
                    {
                        case TlsPacket.Sni sni:
                            yield return (Name: "SNI", Value: String.Join(',', sni.ServerNames));
                            break;
                        case TlsPacket.Alpn alpn:
                            yield return (Name : "ALPN", Value : String.Join(',', alpn.AlpnProtocols));
                            break;
                    }
                }
            }
            var extStrs = CollectionExtensions().Select((n, v) => $"(name={n},value={v})");
            return $"[{String.Join(',', extStrs)}]";
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
