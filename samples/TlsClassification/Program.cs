using System;
using System.Collections.Generic;
using System.Linq;
using Kaitai;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using PacketDotNet;
using PacketDotNet.MiscUtil.Conversion;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
            }
            if (String.Equals("extract", args[0], StringComparison.InvariantCultureIgnoreCase))
            {
                var frameKeyProvider = new FrameKeyProvider();

                var packets = FastPcapFileReaderDevice.ReadAll(args[1]);
                var flows = from packet in packets.Select((p, i) => (Key: frameKeyProvider.GetKey(p), Value: (Number: i, Packet: p)))
                            group packet by packet.Key;

                foreach (var flow in flows.Where(x => IsTlsFlow(x.Key)))
                {
                    Console.WriteLine($"---");
                    Console.WriteLine($"tls-flow:");
                    Console.WriteLine($"  key: '{flow.Key}'");
                    Console.WriteLine($"  records:");

                    byte[] getTcpPayload((int Number, TcpPacket Packet) p)
                    {
                        return p.Packet.PayloadData ?? new byte[0];
                    }

                    var tcpStream = new TcpStream<(int Number, TcpPacket Packet)>(getTcpPayload, flow.Select(f => (f.Value.Number, ParseTcpPacket(f.Value.Packet))));
                    var tlsPackets = ParseTlsPacket(new KaitaiStream(tcpStream));
                    foreach (var tlsRecord in tlsPackets)
                    {
                        var tlsString = TlsDescription(tlsRecord.Packet);
                        Console.WriteLine($"    - {tlsString}");
                        Console.WriteLine($"      segments:");
                        foreach (var entry in tcpStream.GetSegments(tlsRecord.Offset, tlsRecord.Length))
                        {
                            var tcpString = TcpDescription(entry);
                            Console.WriteLine($"      - {tcpString}");
                            var rangeString = TcpRangeString(entry.Range);
                            Console.WriteLine($"        {rangeString}");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        private static string TcpRangeString(Range<long> range)
        {
            return $"range: {{ offset: {range.Min}, length: {(range.Max - range.Min) + 1} }}";
        }

        private static string TcpDescription(((int Number, TcpPacket Packet) Segment, Range<long> Range) entry)
        {
            var sb = new StringBuilder();
            sb.Append($"tcp: {{ number: {entry.Segment.Number},");
            sb.Append($"length: {entry.Segment.Packet.PayloadData?.Length ?? 0},");
            sb.Append($"flags: '{TcpFlags(entry.Segment.Packet)}',");
            sb.Append($"window: {entry.Segment.Packet.WindowSize}");
            sb.Append("}");
            return sb.ToString();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ACTION PCAP_FILE");
            Console.WriteLine();
            Console.WriteLine("Actions:");
            Console.WriteLine("  extract - extracts features for TLS flows in form of YAML document.");         
        }
        private static IEnumerable<(long Offset, int Length, TlsPacket Packet)> ParseTlsPacket(KaitaiStream kaitaiStream)
        {
            var packets = new List<(long Offset, int Length, TlsPacket Packet)>();
            try
            {
                while (!kaitaiStream.IsEof)
                {
                    var tlsOffset = kaitaiStream.Pos;
                    var tlsPacket = new TlsPacket(kaitaiStream);
                    var tlsLength = kaitaiStream.Pos - tlsOffset;
                    packets.Add((tlsOffset, (int)tlsLength, tlsPacket));
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            return packets;
        }

        private static bool IsTlsFlow(FlowKey key)
        {
            return key.Protocol == System.Net.Sockets.ProtocolType.Tcp
                && (key.SourcePort == 443 || key.DestinationPort == 443);
        }
        private static TcpPacket ParseTcpPacket(FrameData frame)
        {
            var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
            var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
            return tcpPacket;
        }

        private static string TcpFlags(TcpPacket packet)
        {
            var flags = new List<string>();
            if(packet.Syn) flags.Add("SYN");
            if(packet.Ack) flags.Add("ACK");
            if(packet.Psh) flags.Add("PSH");
            if(packet.Rst) flags.Add("RST");
            if(packet.Fin) flags.Add("FIN");
            return String.Join(',', flags);
        }
        private static string GetTlsVersion(TlsPacket.TlsVersion tlsVersion)
        {
            if(tlsVersion.Major==3)
                switch(tlsVersion.Minor)
                {
                case 0: return "SSLv3.0";
                case 1: return "TLSv1.0";
                case 2: return "TLSv1.1";
                case 3: return "TLSv1.2";
                case 4: return "TLSv1.3";
                }
            return "SSL";
        }
        private static string ByteString(byte[] bytes)
        {
            return String.Join('-', bytes.Select(x => x.ToString("X")));
        }
        private static string TlsDescription(TlsPacket packet)
        {
            var sb = new StringBuilder();
            sb.Append($"tls: {{ version: {GetTlsVersion(packet.Version)}, ");
            switch (packet.ContentType)
            {
                case TlsPacket.TlsContentType.Handshake:
                    var handshake = packet.Fragment as TlsPacket.TlsHandshake;
                    sb.Append($"protocol: Handshake-{handshake.MsgType}, ");
                    switch(handshake.MsgType)
                    {
                        case TlsPacket.TlsHandshakeType.ClientHello:
                            var clientHello = handshake.Body as TlsPacket.TlsClientHello;
                            sb.Append($"session-id: {ByteString(clientHello.SessionId.Sid)}, ");
                            sb.Append($"cipher-suites: [ {getCiphersString(clientHello.CipherSuites)} ] ");
                            break;
                        case TlsPacket.TlsHandshakeType.ServerHello:
                            var serverHello = handshake.Body as TlsPacket.TlsServerHello;
                            sb.Append($"session-id: {ByteString(serverHello.SessionId.Sid)}, ");
                            sb.Append($"cipher-suite: {(TlsCipherSuite)serverHello.CipherSuite.CipherId} ");
                            break;
                        case TlsPacket.TlsHandshakeType.Certificate:
                            var certificate = handshake.Body as TlsPacket.TlsCertificate;
                            var x509Certificates = certificate.Certificates.Select(x => new X509Certificate2(x.Body));
                            sb.Append("certificates: [");
                            sb.Append(String.Join(',', x509Certificates.Select(x => x.SubjectName.Name)));
                            sb.Append("] ");
                            break;
                    }



                    sb.Append("}}");
                    break;
                case TlsPacket.TlsContentType.ApplicationData:
                    var appdata = packet.Fragment as TlsPacket.TlsApplicationData;
                    sb.Append($"protocol: ApplicationData, ");
                    sb.Append($"length: {appdata.Body.Length} }}");
                    break;
                case TlsPacket.TlsContentType.ChangeCipherSpec:
                    sb.Append($"protocol: ChangeCipherSpec }}");
                    break;
                case TlsPacket.TlsContentType.Alert:
                    sb.Append($"protocol: Alert }}");
                    break;
            }
            return sb.ToString();
        }

        private static string getCiphersString(TlsPacket.CipherSuites cipherSuites)
        {
            return String.Join(',', cipherSuites.CipherSuiteList.Select(x => (TlsCipherSuite)x));
        }
    }
}
