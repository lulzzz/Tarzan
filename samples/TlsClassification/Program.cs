using System;
using System.Linq;
using Tarzan.Nfx.PacketDecoders;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Tarzan.Nfx.Tls;
using Tarzan.Nfx.Utils;
using Tarzan.Nfx.Packets.Common;
using System.Collections.Generic;
using Tarzan.Nfx.Samples.TlsClassification.Writers;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }

            var modelContext = TlsConversationContext.CreateInMemory();

            if (String.Equals("extract", args[0], StringComparison.InvariantCultureIgnoreCase))
            {
                var filepath = args[1];
                var frameKeyProvider = new FrameKeyProvider();
                var keyFile = Path.ChangeExtension(filepath, "key");
                var secretMap = File.Exists(keyFile) ? TlsMasterSecretMap.LoadFromFile(keyFile): new TlsMasterSecretMap();
                var packets = FastPcapFileReaderDevice.ReadAll(args[1]).Select((p, i) => (Key: frameKeyProvider.GetKey(p), Value: (Meta: new PacketMeta { Number = i + 1, Timestamp = p.Timestamp }, Packet: p)));
                var flows = from packet in packets
                            group packet by packet.Key;

                var conversations = TcpStreamConversation.CreateConversations(flows.ToDictionary(x => x.Key, x => x.Select(y => y.Value)));

                foreach (var conversation in conversations)
                {
                    var modelBuilder = new TlsConversationModelBuilder(modelContext);
                    var decoderBuilder = new TlsDecoderBuilder();
                    var processor = new TlsSessionProcessor(modelBuilder, decoderBuilder);
                    processor.ProcessConversation(conversation);


                    var model = modelBuilder.ToModel();
                    modelContext.SaveChanges();


                    var tlsDecoder = decoderBuilder.ToDecoder();
                    var masterSecret = secretMap.GetMasterSecret(ByteString.ByteArrayToString(tlsDecoder.ClientRandom));
                    if (masterSecret != null)
                    {
                        tlsDecoder.MasterSecret = ByteString.StringToByteArray(masterSecret);
                        var tlsSecurityParameters = TlsSecurityParameters.Create(tlsDecoder.ProtocolVersion, tlsDecoder.CipherSuite.ToString(), tlsDecoder.Compression);
                        tlsDecoder.InitializeKeyBlock(tlsSecurityParameters);

                        // USE TLS DECODER
                        DumpConversationContent(tlsDecoder, conversation, processor.ClientDataRecords, processor.ServerDataRecords);
                    }
                }
                CsvFeatureWriter.WriteCsv(Path.ChangeExtension(filepath, "csv"), modelContext);
            }

            

        }

        private static void DumpConversationContent(TlsDecoder tlsDecoder, TcpStreamConversation conversation, IEnumerable<TlsPacket.TlsApplicationData> clientDataRecords, IEnumerable<TlsPacket.TlsApplicationData> serverDataRecords)
        {
            var convKeyString = conversation.ConversationKey.ToString().Replace('>', '_').Replace(':', '_');
            var clientKeys = tlsDecoder.KeyBlock.GetClientKeys();
            foreach (var clientData in clientDataRecords.Select((x, i) => (Data: x, Seqnum: i + 1)))
            {
                DumpApplicationData(tlsDecoder, clientKeys, clientData.Data, (ulong)clientData.Seqnum, $"{convKeyString}-client-{clientData.Seqnum}");
            }
            var serverKeys = tlsDecoder.KeyBlock.GetServerKeys();
            foreach (var serverData in serverDataRecords.Select((x, i) => (Data: x, Seqnum: i + 1)))
            {
                DumpApplicationData(tlsDecoder, serverKeys, serverData.Data, (ulong)serverData.Seqnum, $"{convKeyString}-server-{serverData.Seqnum}");
            }
        }

        private static void DumpApplicationData(TlsDecoder tlsDecoder, TlsKeys tlsKeys, TlsPacket.TlsApplicationData tlsData, ulong seqNumber, string filename)
        {
            var plainBytes = tlsDecoder.DecryptApplicationData(tlsKeys, tlsData, seqNumber);
            if (tlsDecoder.Compression == TlsPacket.CompressionMethods.Deflate)
            {
                plainBytes = tlsDecoder.Decompress(plainBytes);
            }
            File.WriteAllBytes($"{filename}.txt", plainBytes);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ACTION PCAP_FILE");
            Console.WriteLine();
            Console.WriteLine("Actions:");
            Console.WriteLine("  extract - extracts features for TLS flows in form of YAML document.");
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
