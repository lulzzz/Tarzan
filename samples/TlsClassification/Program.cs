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

            var dataCtx = TlsConversationContext.CreateInMemory();

            if (String.Equals("extract", args[0], StringComparison.InvariantCultureIgnoreCase))
            {
                var filepath = args[1];
                var frameKeyProvider = new FrameKeyProvider();
                var secretMap = TlsMasterSecretMap.LoadFromFile(Path.ChangeExtension(filepath, "key"));
                var packets = FastPcapFileReaderDevice.ReadAll(args[1]);
                var flows = from packet in packets.Select((p, i) => (Key: frameKeyProvider.GetKey(p), Value: (Nunber: i, Packet: p)))
                            group packet by packet.Key;
                var flowDict = flows.ToDictionary(x=>x.Key, x=>x.Select(y=>y.Value));
                var conversations = TcpStreamConversation.CreateConversations(flowDict);

                foreach (var conversation in conversations)
                {
                    var processor = new TlsConversationProcessor(dataCtx);
                    processor.ProcessConversation(conversation);

                    var tlsDecoder = processor.Decoder;
                    tlsDecoder.MasterSecret = ByteString.StringToByteArray(secretMap.GetMasterSecret(ByteString.ByteArrayToString(tlsDecoder.ClientRandom)));
                    var tlsSecurityParameters = TlsSecurityParameters.Create(tlsDecoder.ProtocolVersion, tlsDecoder.CipherSuite.ToString(), tlsDecoder.Compression);
                    tlsDecoder.InitializeKeyBlock(tlsSecurityParameters);

                    var convKeyString = conversation.ConversationKey.ToString().Replace('>', '_').Replace(':', '_');
                    var clientKeys = tlsDecoder.KeyBlock.GetClientKeys();
                    foreach (var clientData in processor.ClientDataRecords.Select((x,i)=>(Data: x, Seqnum: i+1)))
                    {
                        DumpApplicationData($"{convKeyString}-client-{clientData.Seqnum}", clientData.Data, (ulong)clientData.Seqnum, clientKeys, tlsDecoder);
                    }
                    var serverKeys = tlsDecoder.KeyBlock.GetServerKeys();
                    foreach (var serverData in processor.ServerDataRecords.Select((x, i) => (Data: x, Seqnum: i+1)))
                    {
                        DumpApplicationData($"{convKeyString}-server-{serverData.Seqnum}", serverData.Data, (ulong)serverData.Seqnum, serverKeys, tlsDecoder);
                    }
                }
            }
        }

        private static void DumpApplicationData(string filename, TlsPacket.TlsApplicationData tlsData, ulong seqNumber, TlsKeys tlsKeys, TlsDecoder tlsDecoder)
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
