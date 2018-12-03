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

    class TlsSessionProcessor
    {
        private readonly ITlsSessionBuilder[] m_builders;

        public TlsSessionProcessor(params ITlsSessionBuilder[] builders)
        {
            m_builders = builders;
        }

        public IEnumerable<TlsPacket.TlsApplicationData> ClientDataRecords { get; private set; }
        public IEnumerable<TlsPacket.TlsApplicationData> ServerDataRecords { get; private set; }

        public void ProcessConversation(TcpStreamConversation conversation)
        {
            foreach(var builder in m_builders) builder.SetFlowKey(conversation.ConversationKey);

            var clientFlow = conversation.Upflow;
            var tlsClientRecordCollection = ParseTlsPacket(new KaitaiStream(clientFlow));
            ClientDataRecords = ProcessRecords(tlsClientRecordCollection, TlsDirection.ClientServer, clientFlow).ToList();

            var serverFlow = conversation.Downflow;
            var tlsServerRecordCollection = ParseTlsPacket(new KaitaiStream(serverFlow));
            ServerDataRecords = ProcessRecords(tlsServerRecordCollection, TlsDirection.ServerClient, serverFlow).ToList();
        }

        public IEnumerable<TlsPacket.TlsApplicationData> ProcessRecords(
            IEnumerable<(Range<long> Range, TlsPacket Packet)> tlsRecordCollection, 
            TlsDirection direction,
            TcpStream<(PacketMeta Meta, TcpPacket Packet)> tcpStream)
        {
            foreach (var tlsRecord in tlsRecordCollection)
            {
                var tcpSegments = tcpStream.GetSegments(tlsRecord.Range.Min, (int)(tlsRecord.Range.Max - tlsRecord.Range.Min)).Select(s => s.Segment).ToList();
                var first = tcpSegments.First();
                var packetContext = new TlsPacketContext
                {
                    direction = direction,
                    recordMeta = first.Meta,
                    tcpPackets = tcpSegments
                };
                switch (tlsRecord.Packet.Fragment)
                {
                    case TlsPacket.TlsHandshake handshake:
                        SetHandshake(handshake, packetContext);
                        break;
                    case TlsPacket.TlsApplicationData applicationData:
                        {
                            foreach(var builder in m_builders) builder.AddApplicationDataRecord(applicationData, packetContext);
                            yield return tlsRecord.Packet.Fragment as TlsPacket.TlsApplicationData;
                        }
                        break;
                }
            }
        }

        private void SetHandshake(TlsPacket.TlsHandshake handshake, TlsPacketContext packetContext)
        {
            switch (handshake.Body)
            {
                case TlsPacket.TlsClientHello clientHello:
                    foreach (var builder in m_builders) builder.SetClientHello(clientHello, packetContext);
                    break;
                case TlsPacket.TlsServerHello serverHello:
                    foreach (var builder in m_builders) builder.SetServerHello(serverHello, packetContext);
                    break;
                case TlsPacket.TlsCertificate certificate:
                    foreach (var builder in m_builders) builder.SetServerCertificate(certificate, packetContext);
                    break;
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
    }
}
