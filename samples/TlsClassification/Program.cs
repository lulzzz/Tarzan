using System;
using System.Collections.Generic;
using System.Linq;
using Kaitai;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using PacketDotNet;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    class Program
    {
        static void Main(string[] args)
        {

            //TlsCipherSuiteName.Test();


            if (args.Length != 2)
            {
                PrintUsage();
            }
            if (String.Equals("extract", args[0], StringComparison.InvariantCultureIgnoreCase))
            {
                var filepath = args[1];
                var frameKeyProvider = new FrameKeyProvider();
                var secretMap = TlsMasterSecretMap.LoadFromFile(Path.ChangeExtension(filepath, "key"));
                var packets = FastPcapFileReaderDevice.ReadAll(args[1]);
                var flows = from packet in packets.Select((p, i) => (Key: frameKeyProvider.GetKey(p), Value: (Number: i, Packet: p)))
                            group packet by packet.Key;

                var conversations = GetConversations(flows);

                foreach (var conv in conversations)
                {

                    var tlsDecoder = new TlsDecoder();
                    var clientData = new List<TlsPacket.TlsApplicationData>();
                    var serverData = new List<TlsPacket.TlsApplicationData>();
                    foreach (var flow in new[] { (Key: conv.Key, Value: conv.Value.UpFlow, AppRecords : clientData),
                                                 (Key: conv.Key.SwapEndpoints(), Value: conv.Value.DownFlow, AppRecords : serverData) })
                    {
                        Console.WriteLine($"---");
                        Console.WriteLine($"tls-flow:");
                        Console.WriteLine($"  key: '{flow.Key}'");
                        Console.WriteLine($"  records:");

                        byte[] getTcpPayload((int Number, TcpPacket Packet) p)
                        {
                            return p.Packet.PayloadData ?? new byte[0];
                        }

                        var tcpStream = new TcpStream<(int Number, TcpPacket Packet)>(getTcpPayload, flow.Value.Select(f => (f.Number, ParseTcpPacket(f.Packet))));
                        var tlsPackets = ParseTlsPacket(new KaitaiStream(tcpStream));

                        foreach (var tlsRecord in tlsPackets)
                        {
                            var tlsString = TlsDescription(tlsRecord.Packet, tlsDecoder, secretMap,  flow.AppRecords);
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
                    tlsDecoder.MasterSecret = ByteString.StringToByteArray(secretMap.GetMasterSecret(ByteString.ByteArrayToString(tlsDecoder.ClientRandom)));
                    var tlsSecurityParameters = TlsSecurityParameters.Create(tlsDecoder.ProtocolVersion, tlsDecoder.CipherSuite.ToString(), tlsDecoder.Compression);
                    tlsDecoder.InitializeKeyBlock(tlsSecurityParameters);

                    foreach (var (name, dataset, tlskeys) in new[] { (Name: "client", Data: clientData, Keys:tlsDecoder.KeyBlock.GetClientKeys()), 
                        (Name: "server", Data: serverData, Keys:tlsDecoder.KeyBlock.GetServerKeys()) })
                    {
                        foreach (var (data, index) in dataset.Select((d, i) => (Data: d, Index: i + 1)))
                        {
                            var plainBytes = tlsDecoder.DecryptApplicationData(tlskeys, data, (ulong)index);
                            if (tlsDecoder.Compression == TlsPacket.CompressionMethods.Deflate)
                            {
                                plainBytes = tlsDecoder.Decompress(plainBytes);
                            }
                            File.WriteAllBytes($"{filepath}-{tlsDecoder.CipherSuite}-{name}-{index}.txt", plainBytes);
                        }
                    }
                }
            }
        }

        private static IEnumerable<KeyValuePair<FlowKey, (IEnumerable<(int Number, FrameData Packet)> UpFlow, IEnumerable<(int Number, FrameData Packet)> DownFlow)>> GetConversations(IEnumerable<IGrouping<FlowKey, (FlowKey Key, (int Number, FrameData Packet) Value)>> flows)
        {
            var flowDictionary = flows.ToDictionary(x => x.Key);
            foreach(var key in flowDictionary.Keys.Where(key => key.SourcePort > key.DestinationPort))
            {
                var upflow = flowDictionary[key].Select(f => f.Value);
                var downflow = flowDictionary[key.SwapEndpoints()].Select(f => f.Value);
                yield return KeyValuePair.Create(key, (upflow,downflow));
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
            //try
            {
                while (!kaitaiStream.IsEof)
                {
                    var tlsOffset = kaitaiStream.Pos;
                    var tlsPacket = new TlsPacket(kaitaiStream);
                    var tlsLength = kaitaiStream.Pos - tlsOffset;
                    packets.Add((tlsOffset, (int)tlsLength, tlsPacket));
                }
            }
            //catch (Exception e)
            {
            //    Console.Error.WriteLine(e);
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

        private static string TlsDescription(TlsPacket packet, TlsDecoder decoder, TlsMasterSecretMap secretMap, List<TlsPacket.TlsApplicationData> dataPackets)
        {
            var version = TlsSecurityParameters.GetSslProtocolVersion(packet.Version.Major, packet.Version.Minor);
            var sb = new StringBuilder();
            sb.Append($"tls: {{ version: {version.ToString()}, ");
            switch (packet.ContentType)
            {
                case TlsPacket.TlsContentType.Handshake:
                    var handshake = packet.Fragment as TlsPacket.TlsHandshake;
                    sb.Append($"protocol: Handshake-{handshake.MsgType}, ");
                    switch(handshake.MsgType)
                    {
                        case TlsPacket.TlsHandshakeType.ClientHello:
                            var clientHello = handshake.Body as TlsPacket.TlsClientHello;
                            sb.Append($"session-id: {ByteString.ByteArrayToString(clientHello.SessionId.Sid)}, ");
                            sb.Append($"client-random: {ByteString.ByteArrayToString(clientHello.Random.RandomBytes)}, ");
                            sb.Append($"cipher-suites: [ {getCiphersString(clientHello.CipherSuites)} ] ");
                            sb.Append($"{getExtensionString(clientHello.Extensions)}");
                            decoder.ClientRandom = ByteString.Combine(clientHello.Random.RandomTime,clientHello.Random.RandomBytes);

                            break;
                        case TlsPacket.TlsHandshakeType.ServerHello:
                            var serverHello = handshake.Body as TlsPacket.TlsServerHello;
                            sb.Append($"session-id: {ByteString.ByteArrayToString(serverHello.SessionId.Sid)}, ");
                            sb.Append($"server-random: {ByteString.ByteArrayToString(serverHello.Random.RandomBytes)}, ");
                            sb.Append($"cipher-suite: {(TlsCipherSuite)serverHello.CipherSuite.CipherId} ");
                            decoder.ServerRandom = ByteString.Combine(serverHello.Random.RandomTime,serverHello.Random.RandomBytes);
                            decoder.CipherSuite = (TlsCipherSuite)serverHello.CipherSuite.CipherId;
                            decoder.ProtocolVersion = version;
                            decoder.Compression = serverHello.CompressionMethod;
                            break;
                        case TlsPacket.TlsHandshakeType.Certificate:
                            var certificate = handshake.Body as TlsPacket.TlsCertificate;
                            var x509Certificates = certificate.Certificates.Select(x => new X509Certificate2(x.Body));
                            sb.Append("certificates: [");
                            sb.Append(String.Join(',', x509Certificates.Select(x => x.SubjectName.Name)));
                            sb.Append("] ");
                            break;
                    }
                    sb.Append("}");
                    break;
                case TlsPacket.TlsContentType.ApplicationData:
                    var appdata = packet.Fragment as TlsPacket.TlsApplicationData;
                    sb.Append($"protocol: ApplicationData, ");
                    sb.Append($"length: {appdata.Body.Length}, ");
                    sb.Append($"content: {ByteString.ByteArrayToString(appdata.Body).Substring(0, 32)} }}");
                    dataPackets.Add(appdata);
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


        private static string getExtensionString(TlsPacket.TlsExtensions extensions)
        {
            
            // server name

            // application layer protocol negotiation


            return String.Empty;
        }

        private static string getCiphersString(TlsPacket.CipherSuites cipherSuites)
        {
            return String.Join(',', cipherSuites.Items.Select(x => (TlsCipherSuite)x));
        }

       




        static void TestDecryptTls()
        {
            ShaPrfAlgorithm.Test100();

            var secret = "64e2d01fa9bd9e7da52377465b6ce5d6e2fe37517d54199ed4d2714b4741494c7a702f972fd8d23a94ef89d9f0c3a880";
            var clientRandom = "029b68c172bc58b0463396de16b69a64f49109a1af6e8ce177aabd7323645693";
            var serverRandom = "79b72bc1cb4465b284b8796b65b08a4b6d8d741b36b5d75634ce612345e6f744";
            var cipherBytes = new Span<byte>(File.ReadAllBytes("encrypted.raw"));

            var prf = new ShaPrfAlgorithm();
            var keyBlockBytes = prf.GetSecretBytes(ByteString.StringToByteArray(secret),
                                                 "key expansion",
                                                 ByteString.Combine(ByteString.StringToByteArray(serverRandom), ByteString.StringToByteArray(clientRandom)), 40);
            var keyBlock = new TlsKeyBlock(keyBlockBytes, 0, 16, 4);

            var sequenceNumber = 1ul;

            var fixedNonce = keyBlock.ClientIV.Slice(0, 4);
            var recordNonce = cipherBytes.Slice(0, 8);

            // nonce = client_iv + sequence_id
            var nonce = ByteString.Combine(fixedNonce.ToArray(), recordNonce.ToArray());

            // additional_data = seq_num + TLSCompressed.type + TLSCompressed.version + TLSCompressed.length;
            // TLSCompressed.length = Length - recordIv.size - Mac.size
            var additionalData = ByteString.Combine(
                BitConverter.GetBytes(sequenceNumber).Reverse().ToArray(), 
                new byte[] { 0x17, 0x03, 0x03, 0x01, 0xc7 - (8 + 16) });

            var gsm = new GcmBlockCipher(new AesEngine());
            var plainBytes = TlsDecoder.DecryptAead(gsm ,keyBlock.ClientWriteKey, nonce, cipherBytes.Slice(8), additionalData);
            Console.WriteLine(Encoding.ASCII.GetString(plainBytes)); 
        }
    }
}
