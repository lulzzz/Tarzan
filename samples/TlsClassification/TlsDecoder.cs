using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Samples.TlsClassification
{

    /// <summary>
    /// The decoder for TLS communication that can extract plain text from TLS communication 
    /// if premaster secret is known. See https://sharkfesteurope.wireshark.org/assets/presentations17eu/15.pdf
    /// </summary>
    public class TlsDecoder
    {
        /// <summary>
        /// Gets or sets the master secret.
        /// </summary>
        /// <value>The master secret.</value>
        public byte[] MasterSecret { get; set; }
        /// <summary>
        /// Gets or sets the client random.
        /// </summary>
        /// <value>The client random.</value>
        public byte[] ClientRandom { get; set; }
        /// <summary>
        /// Gets or sets the server random.
        /// </summary>
        /// <value>The server random.</value>
        public byte[] ServerRandom { get; set; }
        /// <summary>
        /// Gets or sets the protocol version.
        /// </summary>
        /// <value>The protocol version.</value>
        public SslProtocols ProtocolVersion { get; set; }
        /// <summary>
        /// Gets or sets the cipher suite.
        /// </summary>
        /// <value>The cipher suite.</value>
        public TlsCipherSuite CipherSuite { get; set; }

        /// <summary>
        /// Gets or sets the security parameters.
        /// </summary>
        /// <value>The security parameters.</value>
        public TlsSecurityParameters SecurityParameters { get; set; }

        /// <summary>
        /// Gets the key block.
        /// </summary>
        /// <value>The key block.</value>
        public TlsKeyBlock KeyBlock { get; private set;}
        public TlsPacket.CompressionMethods Compression { get; internal set; }

        public TlsDecoder()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:TlsClassification.TlsDecoder"/> class.
        /// </summary>
        /// <param name="masterSecret">Master secret bytes, lenght is 48.</param>
        /// <param name="clientRandom">Client random bytes, length is 32.</param>
        /// <param name="serverRandom">Server random bytes, length is 32.</param>
        public TlsDecoder(byte[] masterSecret, byte[] clientRandom, byte[] serverRandom)
        {
            MasterSecret = masterSecret;
            ClientRandom = clientRandom;
            ServerRandom = serverRandom;
        }


        public void InitializeKeyBlock(TlsSecurityParameters securityParameters)
        {
            SecurityParameters = securityParameters;
            // key_block = PRF(SecurityParameters.master_secret, "key expansion",
            // SecurityParameters.server_random + SecurityParameters.client_random);
            var bytes = securityParameters.Prf.GetSecretBytes(MasterSecret, "key expansion",
                                                                       ByteString.Combine(ServerRandom, ClientRandom),
                                                                       securityParameters.KeyMaterialSize/8);
                                                                       
            KeyBlock = new TlsKeyBlock(bytes, securityParameters.MacKeyLength, securityParameters.EncodingKeyLength, securityParameters.FixedIVLength);
        }

        /// <summary>
        /// Decodes the AES-GCM encrypted data according to RFC5288 and RFC5264.
        /// (https://tools.ietf.org/html/rfc5288, https://tools.ietf.org/html/rfc5246#page-24).
        /// </summary>
        public static byte[] DecryptAead(IAeadBlockCipher gcm, Span<byte> writeKeyBytes, Span<byte> nonceBytes, Span<byte> encryptedBytes, Span<byte> additionalData)
        {
            var writeKey = new KeyParameter(writeKeyBytes.ToArray());

            gcm.Init(false, new AeadParameters(writeKey, 128, nonceBytes.ToArray(), additionalData.ToArray()));

            var outsize = gcm.GetOutputSize(encryptedBytes.Length);
            var plainBytes = new byte[outsize];
            var outBytes = gcm.ProcessBytes(encryptedBytes.ToArray(), 0, encryptedBytes.Length, plainBytes, 0);
            // finally check MAC:
            outBytes += gcm.DoFinal(plainBytes, outBytes);
            return plainBytes;
        }

        public byte[] Decompress(byte[] plainBytes)
        {
            var ms = DecompressBytes(plainBytes);
            return ms.ToArray();
        }

        private static MemoryStream DecompressBytes(byte[] input)
        {
            var output = new MemoryStream();

            using (var compressStream = new MemoryStream(input))
            using (var decompressor = new DeflateStream(compressStream, CompressionMode.Decompress))
                decompressor.CopyTo(output);

            output.Position = 0;
            return output;
        }

        /// <summary>
        /// Decrypts bytes using the block cipher and CBC methods (https://tools.ietf.org/html/rfc2246#section-6.2.3.2).
        /// </summary>
        /// <returns>The block.</returns>
        /// <param name="writeKey">Write key.</param>
        /// <param name="initializationVector">Nonce, which stands for .</param>
        /// <param name="macKey">Span.</param>
        public static byte[] DecryptBlock(IBlockCipher cbc, HMAC hmac, Span<byte> writeKey, Span<byte> initializationVector, Span<byte> macKey, Span<byte> encryptedBytes)
        {
            //  opaque IV[SecurityParameters.record_iv_length];  ???
            //  block-ciphered struct {
            //    opaque content[TLSCompressed.length];
            //    opaque MAC[SecurityParameters.mac_length];
            //    uint8 padding[GenericBlockCipher.padding_length];
            //    uint8 padding_length;
            //  } GenericBlockCipher;
            // first decrypt:
            //

            var blockSize = cbc.GetBlockSize();
            cbc.Init(false, new ParametersWithIV(new KeyParameter(writeKey.ToArray()), initializationVector.ToArray()));
            var outputBuffer = new byte[encryptedBytes.Length];
            var encryptedBytesArray = encryptedBytes.ToArray();
            for (var block = 0; block < encryptedBytes.Length / blockSize; block++) 
                cbc.ProcessBlock(encryptedBytesArray, block * blockSize, outputBuffer, block * blockSize);
            // check padding:
            var paddingLen = outputBuffer[outputBuffer.Length - 1];
            var contentLen = encryptedBytes.Length - (1 + paddingLen + (hmac.HashSize / 8)) - blockSize;

            return new Span<byte>(outputBuffer).Slice(blockSize, contentLen).ToArray();
        }

        

        public byte[] DecryptApplicationData(TlsKeys tlsKeys, TlsPacket.TlsApplicationData applicationData, ulong sequenceNumber)
        {
            if (KeyBlock == null) throw new InvalidOperationException($"KeyBlock not initialized. Please, call {nameof(InitializeKeyBlock)} first.");
            var writeKey = tlsKeys.EncodingKey;
            var mackKey = tlsKeys.MacKey;
            var fixedNonce = new Span<byte>(tlsKeys.IV).Slice(0, SecurityParameters.FixedIVLength / 8);

            var content = new Span<byte>(applicationData.Body);

            var macLength = SecurityParameters.MacLength / 8;
            var recordNonceLength = SecurityParameters.RecordIVLength / 8;
            var recordNonce = content.Slice(0, recordNonceLength);

            var nonce = ByteString.Combine(fixedNonce.ToArray(), recordNonce.ToArray());

            var additionalData = ByteString.Combine(
                    BitConverter.GetBytes(sequenceNumber).Reverse().ToArray(),
                    new byte[] { (byte)applicationData.M_Parent.ContentType,
                                       applicationData.M_Parent.Version.Major,
                                       applicationData.M_Parent.Version.Minor },
                    BitConverter.GetBytes((ushort)(applicationData.Body.Length - (recordNonceLength + macLength))).Reverse().ToArray()
                );

            if (this.SecurityParameters.CipherType == TlsCipherType.Aead)
            {
                var aead = CreateAeadCipher(SecurityParameters.CipherMode, CreateBlockCipher(SecurityParameters.CipherAlgorithm.ToString().ToUpperInvariant()));
                return DecryptAead(aead, writeKey, nonce, content.Slice(recordNonceLength), additionalData);
            }
            if (this.SecurityParameters.CipherType == TlsCipherType.Block)
            {
                var cbc = CreateBlockCipher(SecurityParameters.CipherMode, CreateBlockCipher(SecurityParameters.CipherAlgorithm.ToString().ToUpperInvariant()));
                var mac = CreateHMacAlgorithm(SecurityParameters.MacAlgorithm);
                return DecryptBlock(cbc, mac, writeKey, fixedNonce, mackKey, content.Slice(recordNonceLength));
            }
            if (this.SecurityParameters.CipherType == TlsCipherType.Stream)
            {
                throw new NotImplementedException();
            }
            throw new NotSupportedException($"Decrypting {CipherSuite.ToString()} is not supported.");
        }

        private HMAC CreateHMacAlgorithm(string macAlgorithm)
        {
            switch(macAlgorithm)
            {
                case "SHA":
                case "SHA1":
                    return new HMACSHA1();
                case "SHA256":
                    return new HMACSHA256();
                case "SHA384":
                    return new HMACSHA384();
                case "SHA512":
                    return new HMACSHA512();
                case "MD5":
                    return new HMACMD5();
            }
            return new HMACSHA1();
        }

        private IBlockCipher CreateBlockCipher(TlsCipherMode cipherMode, IBlockCipher blockCipher)
        {
            switch (cipherMode)
            {
                case TlsCipherMode.CBC:
                case TlsCipherMode.EDE_CBC:
                    return new CbcBlockCipher(blockCipher);
                
            }
            return null;
        }
        private IAeadBlockCipher CreateAeadCipher(TlsCipherMode cipherMode, IBlockCipher blockCipher)
        {
            switch(cipherMode)
            {
                case TlsCipherMode.CCM_8:
                case TlsCipherMode.CCM:
                    return new CcmBlockCipher(blockCipher);
                case TlsCipherMode.GCM:
                    return new GcmBlockCipher(blockCipher);
            }
            return null;
        }

        private IBlockCipher CreateBlockCipher(string cipherAlgorithm)
        {
            switch (cipherAlgorithm)
            {
                case "AES128":
                case "AES256":
                case "AES":
                    return new AesEngine();
                case "DES":
                    return new DesEngine();
                case "3DES":
                    return new DesEdeEngine();
                case "RC2":
                    return new RC2Engine();
                case "IDEA":
                    return new IdeaEngine();
                case "CAMELLIA":
                    return new CamelliaEngine();
                case "SEED":
                    return new SeedEngine();

            }
            return null;
        }

        private IStreamCipher CreateStreamCipher(string cipherAlgorithm)
        {
            switch (cipherAlgorithm)
            {
                case "RC4":
                    return new RC4Engine();
            }
            return null;
        }
    }
}
