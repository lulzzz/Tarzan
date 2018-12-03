using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PacketDotNet;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using Tarzan.Nfx.Tls;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public class TlsConversationModelBuilder : ITlsSessionBuilder
    {
        private TlsConversationModel m_conversationModel;
        private readonly TlsConversationContext m_modelContext;

        public TlsConversationModel Model => m_conversationModel;

        public TlsConversationModelBuilder(TlsConversationContext modelContext)
        {
            m_conversationModel = new TlsConversationModel
            {
                Records = new List<TlsRecordModel>()
            };
            modelContext.Add(m_conversationModel);
            m_modelContext = modelContext;
        }

        public void SetFlowKey(FlowKey flowKey)
        {
            m_conversationModel.ConversationKey = flowKey.ToString();
        }

        public void SetClientHello(TlsPacket.TlsClientHello clientHello, TlsPacketContext packetContext)
        {
            string GetCipherSuites(TlsPacket.CipherSuites cipherSuites)
            {
                var suites = cipherSuites.Items.Select(x => ((TlsCipherSuite)x).ToString());
                return $"[{String.Join(',', suites)}]";
            }

            m_conversationModel.SessionId = ByteString.ByteArrayToString(clientHello.SessionId.Sid);
            m_conversationModel.ClientRandom = ByteString.ByteArrayToString(clientHello.Random.RandomBytes);
            m_conversationModel.ClientCipherSuites = GetCipherSuites(clientHello.CipherSuites);
            m_conversationModel.ClientExtensions = GetExtensions(clientHello.Extensions);
            m_conversationModel.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(packetContext.recordMeta.Timestamp);
        }

        public void SetServerHello(TlsPacket.TlsServerHello serverHello, TlsPacketContext packetContext)
        {
            m_conversationModel.Version = TlsSecurityParameters.GetSslProtocolVersion(serverHello.Version.Major, serverHello.Version.Minor).ToString(); 
            m_conversationModel.SessionId = ByteString.ByteArrayToString(serverHello.SessionId.Sid);
            m_conversationModel.ServerRandom = ByteString.ByteArrayToString(serverHello.Random.RandomBytes);
            m_conversationModel.ServerCipherSuite = $"{(TlsCipherSuite)serverHello.CipherSuite.CipherId}";
            m_conversationModel.ServerExtensions = GetExtensions(serverHello.Extensions);
        }

        public TlsConversationModel ToModel()
        {
            return this.m_conversationModel;
        }

        public void SetVersion(SslProtocols version)
        {
            m_conversationModel.Version = version.ToString();
        }

        public void SetServerCertificate(TlsPacket.TlsCertificate certificate, TlsPacketContext packetContext)
        {
            TlsCertificateModel CreateCertificate(X509Certificate2 cert)
            {
                var newCertificateModel = new TlsCertificateModel
                {
                    SubjectName = cert.SubjectName.Name,
                    IssuerName = cert.IssuerName.Name,
                    NotBefore = cert.NotBefore,
                    NotAfter = cert.NotAfter
                };
                m_modelContext.Add(newCertificateModel);
                return newCertificateModel;
            }

            var x509Certificates = certificate.Certificates.Select(x => new X509Certificate2(x.Body));
            m_conversationModel.ServerCertificates = x509Certificates.Select(CreateCertificate).ToList();
        }

        /// <summary>
        /// Adds new TLS record to the conversation model.
        /// </summary>
        /// <param name="applicationData">The application data record.</param>
        /// <param name="direction">The direction, i.e., client to server or vice versa.</param>
        /// <param name="recordMeta">Metadata of the TLS record.</param>
        /// <param name="tcpPackets">A collection of TCP segments caryying the record's data.</param>
        public void AddApplicationDataRecord(TlsPacket.TlsApplicationData applicationData, TlsPacketContext packetContext)
        {
            TcpSegmentModel CreateModel((PacketMeta Meta, TcpPacket Packet) packet)
            {
                var newSegmentModel = new TcpSegmentModel
                {
                    TimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(packet.Meta.Timestamp) - m_conversationModel.Timestamp,
                    PacketId = packet.Meta.Number,
                    Flags = TcpFlags(packet.Packet),
                    Length = packet.Packet.PayloadData?.Length ?? 0,
                    Window = packet.Packet.WindowSize
                };
                m_modelContext.Add(newSegmentModel);
                return newSegmentModel;
            }

            var newRecordModel = new TlsRecordModel
            {
                RecordId = packetContext.recordMeta.Number,
                Direction = packetContext.direction,
                TimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(packetContext.recordMeta.Timestamp) - m_conversationModel.Timestamp,
                Length = applicationData.Body.Length,
                Segments = packetContext.tcpPackets.Select(CreateModel).ToList(),
            };
            m_modelContext.Add(newRecordModel);
            m_conversationModel.Records.Add(newRecordModel);
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
                            yield return (Name: "SNI", Value: String.Join(',', sni.ServerNames.Select(s=>Encoding.ASCII.GetString(s.HostName))));
                            break;
                        case TlsPacket.Alpn alpn:
                            yield return (Name : "ALPN", Value : String.Join(',', alpn.AlpnProtocols.Select(s=>Encoding.ASCII.GetString(s.Name))));
                            break;
                    }
                }
            }
            var extStrs = CollectionExtensions().Select(x => $"(name={x.Name},value={x.Value})");
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
