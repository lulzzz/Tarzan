using System;
using System.Collections;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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


        public static TlsSecurityParameters GetSecurityParameters(SslProtocols version, TlsCipherSuite suite)
        {
            var suiteString = suite.ToString();

            var cipherAlgorithm = GetCipherAlgorithm(suiteString);
            var hashAlgorithm = GetHashAlgorithm(suiteString);
            var cipherType = GetCipherType(suiteString);
            return new TlsSecurityParameters
            {
                CipherAlgorithm = cipherAlgorithm,
                MacAlgorithm = hashAlgorithm,
                CipherType = cipherType,
                CommpressionMethod = TlsSecurityParameters.TlsCommpressionMethod.Null,
                EncodingKeyLength = GetEncodingKeyLength(cipherAlgorithm),
                BlockLength = GetBlockLength(cipherAlgorithm),
                MacKeyLength = GetMacKeyLength(hashAlgorithm),
                MacLength = GetMacLength(hashAlgorithm),
                PrfAlgorithm = GetPrfAlgorithm(version),
                FixedIVLength = GetFixedVectorLength(cipherAlgorithm, cipherType),
                RecordIVLength = GetRecordVectorLength(cipherAlgorithm, cipherType)
            };
        }

        /// <summary>
        /// Gets the length in bits of the initialization vector carried explicitly in the record (usually 8 bytes which is 64bits).
        /// </summary>
        public static int GetRecordVectorLength(CipherAlgorithmType cipherAlgorithm, TlsSecurityParameters.TlsCipherType cipherType)
        {
            if (cipherType == TlsSecurityParameters.TlsCipherType.Stream) return 0;

            switch(cipherAlgorithm)
            {
                case CipherAlgorithmType.Rc4: return 0;
                case CipherAlgorithmType.TripleDes: return 64;
                case CipherAlgorithmType.Aes128: 
                case CipherAlgorithmType.Aes192: 
                case CipherAlgorithmType.Aes256: return 64;
                default: return 0;
            }
        }

        /// <summary>
        /// Gets the length in bits of the fixed part of the initialization vector. This is usually computed 
        /// from the premaster key. Default is 32 bits.
        /// </summary>
        public static int GetFixedVectorLength(CipherAlgorithmType cipherAlgorithm, TlsSecurityParameters.TlsCipherType cipherType)
        {
            if (cipherType == TlsSecurityParameters.TlsCipherType.Stream) return 0;

            switch (cipherAlgorithm)
            {
                case CipherAlgorithmType.Rc4: return 0;
                case CipherAlgorithmType.TripleDes: return 32;
                case CipherAlgorithmType.Aes128:
                case CipherAlgorithmType.Aes192:
                case CipherAlgorithmType.Aes256: return 32;
                default: return 0;
            }
        }

        public static PrfAlgorithm GetPrfAlgorithm(SslProtocols version, HashAlgorithmType hash = HashAlgorithmType.None)
        {
            if (version == SslProtocols.Tls11)
                return new LegacyPrfAlgorithm();
            if (version == SslProtocols.Tls12)
            {
                if (hash != HashAlgorithmType.None)
                    return new ShaPrfAlgorithm(KeyedHashAlgorithm.Create(hash.ToString()));
                else
                    return new ShaPrfAlgorithm();
            }
            return null;
        }

        /// <summary>
        /// Gets the length in bits of the mac for the specified hash algorithm.
        /// </summary>
        /// <returns>The mac length.</returns>
        /// <param name="hashAlgorithm">Hash algorithm.</param>
        public static int GetMacLength(HashAlgorithmType hashAlgorithm)
        {
            switch(hashAlgorithm)
            {
                case HashAlgorithmType.Md5: return 128;
                case HashAlgorithmType.Sha1: return 160;
                case HashAlgorithmType.Sha256: return 256;
                case HashAlgorithmType.Sha384: return 384;
                case HashAlgorithmType.Sha512: return 512;
                default: return 0;
            }
        }

        public static int GetMacKeyLength(HashAlgorithmType hashAlgorithm)
        {
            return GetMacLength(hashAlgorithm);
        }

        /// <summary>
        /// Gets the length in bits of the block cipher. For stream 
        /// cipher the value is equal to 1 (case of RC4).
        /// </summary>
        /// <returns>The block length.</returns>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        public static int GetBlockLength(CipherAlgorithmType cipherAlgorithm)
        {
            switch (cipherAlgorithm)
            {
                case CipherAlgorithmType.Aes128: return 128;
                case CipherAlgorithmType.Aes192: return 192;
                case CipherAlgorithmType.Aes256: return 256;
                case CipherAlgorithmType.Des: return 64;
                case CipherAlgorithmType.TripleDes: return 64;
                case CipherAlgorithmType.Rc2: return 64;
                case CipherAlgorithmType.Rc4: return 1;
                default: return 128;
            }
        }

        /// <summary>
        /// Gets the length in bits of the encoding key for cipher algorithm.
        /// </summary>
        /// <returns>The encoding key length.</returns>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        public static int GetEncodingKeyLength(CipherAlgorithmType cipherAlgorithm)
        {
            switch(cipherAlgorithm)
            {
                case CipherAlgorithmType.Aes128: return 128;
                case CipherAlgorithmType.Aes192: return 192;
                case CipherAlgorithmType.Aes256: return 256;
                case CipherAlgorithmType.Des: return 64;
                case CipherAlgorithmType.TripleDes: return 192;
                case CipherAlgorithmType.Rc2: return 64;
                case CipherAlgorithmType.Rc4: return 128;
                default: return 128;
            }
        }



        public static HashAlgorithmType GetHashAlgorithm(string suiteString)
        {
            if (suiteString.Contains("MD5"))    return HashAlgorithmType.Md5;
            if (suiteString.Contains("SHA256")) return HashAlgorithmType.Sha256;
            if (suiteString.Contains("SHA384")) return HashAlgorithmType.Sha384;
            if (suiteString.Contains("SHA512")) return HashAlgorithmType.Sha512;
            if (suiteString.Contains("SHA"))    return HashAlgorithmType.Sha1;
            return HashAlgorithmType.None;
        }

        public static CipherAlgorithmType GetCipherAlgorithm(string cipherString)
        {
            if (cipherString.Contains("WITH_NULL")) return CipherAlgorithmType.Null;
            if (cipherString.Contains("WITH_AES_128")) return CipherAlgorithmType.Aes128;
            if (cipherString.Contains("WITH_AES_192")) return CipherAlgorithmType.Aes192;
            if (cipherString.Contains("WITH_AES_256")) return CipherAlgorithmType.Aes256;

            if (cipherString.Contains("WITH_DES")) return CipherAlgorithmType.Des;
            if (cipherString.Contains("WITH_RC2")) return CipherAlgorithmType.Rc2;
            if (cipherString.Contains("WITH_RC4")) return CipherAlgorithmType.Rc4;
            if (cipherString.Contains("WITH_3DES")) return CipherAlgorithmType.TripleDes;
            return CipherAlgorithmType.None;
        }

        internal static SslProtocols GetSslProtocolVersion(int major, int minor)
        {
            switch(major)
            {
                case 2: return SslProtocols.Ssl2;
                case 3:
                    switch(minor)
                    {
                        case 0: return SslProtocols.Ssl3;
                        case 1: return SslProtocols.Tls;
                        case 2: return SslProtocols.Tls11;
                        case 3: return SslProtocols.Tls12;
                    }
                    break;
            }
            return SslProtocols.None;
        }

        /// <summary>
        /// Gets the type of the cipher.
        /// </summary>
        /// <returns>The cipher type.</returns>
        /// <param name="cipherString">Cipher string.</param>
        /// <remarks>
        /// 
        /// Two major modes are used in TLSv1.2:
        /// 
        /// In CBC mode, you encrypt a block of data by taking the current plaintext 
        /// block and exclusive-oring that with the previous ciphertext block (or IV), 
        /// and then sending the result of that through the block cipher; 
        /// the output of the block cipher is the ciphertext block.
        /// 
        /// GCM mode provides both privacy (encryption) and integrity. To provide encryption, 
        /// GCM maintains a counter; for each block of data, it sends the current value of 
        /// the counter through the block cipher. Then, it takes the output of the block cipher, 
        /// and exclusive or's that with the plaintext to form the ciphertext.
        /// 
        /// CCM is CBC-MAC mode that provides both authentication and confidentiality.
        /// CCM uses 16-octet authentication tag while CCM_8 uses shorter, 8-octet tag.
        /// </remarks>
        public static TlsSecurityParameters.TlsCipherType GetCipherType(string cipherString)
        {
            // Block (RFC5246-6.2.3.2): CBC
            if (cipherString.Contains("CBC")) return TlsSecurityParameters.TlsCipherType.Block;

            // AEAD (RFC5246-6.2.3.3): CCM, GCM
            if (cipherString.Contains("CCM_8")) return TlsSecurityParameters.TlsCipherType.Aead;
            if (cipherString.Contains("CCM")) return TlsSecurityParameters.TlsCipherType.Aead;
            if (cipherString.Contains("GCM")) return TlsSecurityParameters.TlsCipherType.Aead;

            // Stream (RFC5246-6.2.3.1): 
            return TlsSecurityParameters.TlsCipherType.Stream;
        }

        public void InitializeKeyBlock(TlsSecurityParameters securityParameters)
        {
            SecurityParameters = securityParameters;
            // key_block = PRF(SecurityParameters.master_secret, "key expansion",
            // SecurityParameters.server_random + SecurityParameters.client_random);
            var bytes = securityParameters.PrfAlgorithm.GetSecretBytes(MasterSecret, "key expansion",
                                                                       ByteString.Combine(ServerRandom, ClientRandom),
                                                                       securityParameters.KeyMaterialSize);
                                                                       
            KeyBlock = new TlsKeyBlock(bytes, securityParameters.MacKeyLength, securityParameters.EncodingKeyLength, securityParameters.FixedIVLength);
        }


        /// <summary>
        /// Decodes the AES-CCM encrypted data according to RFC6655
        /// (https://tools.ietf.org/html/rfc6655).
        /// </summary>
        /// <returns>The decoded plain text or null.</returns>
        /// <param name="keyBytes">Key bytes.</param>
        /// <param name="initializationVector">Initialization vector.</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        /// <param name="cipherBytes">Cipher text.</param>
        public static byte[] DecodeAesCcm128(Span<byte> keyBytes, Span<byte> nonceBytes, Span<byte> encryptedBytes, Span<byte> additionalData)
        {
            var aes = new AesLightEngine();
            var ccm = new CcmBlockCipher(aes);
            var key = new KeyParameter(keyBytes.ToArray());

            ccm.Init(false, new AeadParameters(key, 128, nonceBytes.ToArray(), additionalData.ToArray()));

            var outsize = ccm.GetOutputSize(encryptedBytes.Length);
            var plainBytes = new byte[outsize];
            var outBytes = ccm.ProcessBytes(encryptedBytes.ToArray(), 0, encryptedBytes.Length, plainBytes, 0);

            outBytes += ccm.DoFinal(plainBytes, outBytes);
            return plainBytes;
        }
        /// <summary>
        /// Decodes the AES-GCM encrypted data according to RFC5288 and RFC5264.
        /// (https://tools.ietf.org/html/rfc5288, https://tools.ietf.org/html/rfc5246#page-24).
        /// </summary>
        public static byte[] DecryptAesGcm128(Span<byte> writeKeyBytes, Span<byte> nonceBytes, Span<byte> encryptedBytes, Span<byte> additionalData)
        {

            var aes = new AesEngine();
            var gcm = new GcmBlockCipher(aes);
            var key = new KeyParameter(writeKeyBytes.ToArray());

            gcm.Init(false, new AeadParameters(key, 128, nonceBytes.ToArray(), additionalData.ToArray()));

            var outsize = gcm.GetOutputSize(encryptedBytes.Length);
            var plainBytes = new byte[outsize];
            var outBytes = gcm.ProcessBytes(encryptedBytes.ToArray(), 0, encryptedBytes.Length, plainBytes, 0);
            // finally check MAC:
            outBytes += gcm.DoFinal(plainBytes, outBytes);
            return plainBytes;
        }


        public byte[] DecryptApplicationData(bool clientWrite, TlsPacket.TlsApplicationData applicationData, ulong sequenceNumber)
        {
            if (KeyBlock == null) throw new InvalidOperationException($"KeyBlock not initialized. Please, call {nameof(InitializeKeyBlock)} first.");
            var writeKey = clientWrite ? KeyBlock.ClientWriteKey : KeyBlock.ServerWriteKey;

            var fixedNonce = clientWrite ? KeyBlock.ClientIV.Slice(0, SecurityParameters.FixedIVLength / 8) : KeyBlock.ServerIV.Slice(0, SecurityParameters.FixedIVLength / 8);
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
                BitConverter.GetBytes(applicationData.M_Parent.Length - (recordNonceLength + macLength)).Reverse().ToArray()
            );
            return DecryptAesGcm128(writeKey, nonce, content.Slice(recordNonceLength), additionalData);
        }
    }
}
