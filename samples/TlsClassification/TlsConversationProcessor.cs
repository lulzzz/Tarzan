using System.Collections.Generic;
using Kaitai;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using PacketDotNet;
using Tarzan.Nfx.Tls;
using Tarzan.Nfx.Utils;
using System.Linq;

namespace Tarzan.Nfx.Samples.TlsClassification
{

    class TlsConversationProcessor
    {
        private IEnumerable<TlsPacket.TlsApplicationData> m_clientDataRecords;
        private IEnumerable<TlsPacket.TlsApplicationData> m_serverDataRecords;

        private TlsDecoder m_tlsDecoder = new TlsDecoder();
        private TlsFlowModelBuilder m_tlsModelBuilder = new TlsFlowModelBuilder();

        public TlsDecoder Decoder { get => m_tlsDecoder; }
        public TlsFlowModel FlowData { get => m_tlsModelBuilder.FlowData; }
        public IEnumerable<TlsPacket.TlsApplicationData> ClientDataRecords { get => m_clientDataRecords; }
        public IEnumerable<TlsPacket.TlsApplicationData> ServerDataRecords { get => m_serverDataRecords; }

        public void ProcessConversation(TcpStreamConversation conversation)
        {
            var clientFlow = conversation.Upflow;
            var tlsClientRecordCollection = ParseTlsPacket(new KaitaiStream(clientFlow));
            m_clientDataRecords = ProcessRecords(tlsClientRecordCollection, TlsDirection.ClientServer, clientFlow);

            var serverFlow = conversation.Upflow;
            var tlsServerRecordCollection = ParseTlsPacket(new KaitaiStream(clientFlow));
            m_serverDataRecords = ProcessRecords(tlsServerRecordCollection, TlsDirection.ServerClient, clientFlow);
        }

        public IEnumerable<TlsPacket.TlsApplicationData> ProcessRecords(IEnumerable<(Range<long> Range, TlsPacket Packet)> tlsRecordCollection, 
            TlsDirection direction,
            TcpStream<(PacketMeta Meta, TcpPacket Packet)> tcpStream)
        {
            foreach (var tlsRecord in tlsRecordCollection)
            {
                switch (tlsRecord.Packet.ContentType)
                {
                    case TlsPacket.TlsContentType.Handshake:
                        SetHandshake(tlsRecord.Packet.Fragment as TlsPacket.TlsHandshake);
                        break;
                    case TlsPacket.TlsContentType.ApplicationData:
                        {
                            var applicationData = tlsRecord.Packet.Fragment as TlsPacket.TlsApplicationData;
                            var tcpSegments = tcpStream.GetSegments(tlsRecord.Range.Min, (int)(tlsRecord.Range.Max - tlsRecord.Range.Min)).Select(s=>s.Segment).ToList();
                            var first = tcpSegments.First();
                            m_tlsModelBuilder.SetApplicationData(applicationData, direction, first.Meta, tcpSegments);
                            yield return tlsRecord.Packet.Fragment as TlsPacket.TlsApplicationData;
                        }
                        break;
                }
            }
        }

        private IEnumerable<(Range<long>, TlsPacket Packet)> ParseTlsPacket(KaitaiStream kaitaiStream)
        {
            var packets = new List<(Range<long>, TlsPacket Packet)>();
            //try
            {
                while (!kaitaiStream.IsEof)
                {
                    var tlsOffset = kaitaiStream.Pos;
                    var tlsPacket = new TlsPacket(kaitaiStream);
                    packets.Add((new Range<long>(tlsOffset, kaitaiStream.Pos), tlsPacket));
                }
            }
            //catch (Exception e)
            {
                //    Console.Error.WriteLine(e);
            }
            return packets;
        }

        private bool IsTlsFlow(FlowKey key)
        {
            return key.Protocol == System.Net.Sockets.ProtocolType.Tcp
                && (key.SourcePort == 443 || key.DestinationPort == 443);
        }

        private void SetHandshake(TlsPacket.TlsHandshake handshake)
        {
            switch (handshake.MsgType)
            {
                case TlsPacket.TlsHandshakeType.ClientHello:
                    var clientHello = handshake.Body as TlsPacket.TlsClientHello;
                    m_tlsDecoder.ClientRandom = ByteString.Combine(clientHello.Random.RandomTime, clientHello.Random.RandomBytes);
                    m_tlsModelBuilder.SetFromClientHello(clientHello);
                    break;
                case TlsPacket.TlsHandshakeType.ServerHello:
                    var serverHello = handshake.Body as TlsPacket.TlsServerHello;
                    m_tlsDecoder.ServerRandom = ByteString.Combine(serverHello.Random.RandomTime, serverHello.Random.RandomBytes);
                    m_tlsDecoder.CipherSuite = (TlsCipherSuite)serverHello.CipherSuite.CipherId;
                    m_tlsDecoder.Compression = serverHello.CompressionMethod;
                    m_tlsModelBuilder.SetFromServerHello(serverHello);
                    break;
                case TlsPacket.TlsHandshakeType.Certificate:
                    var certificate = handshake.Body as TlsPacket.TlsCertificate;
                    m_tlsModelBuilder.SetServerCertificate(certificate);
                    break;
            }
        }
    }
}
