using System;
using System.IO;
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
        public byte[] MasterSecret { get; set;  }
        public byte[] ClientRandom { get; set;  }
        public byte[] ServerRandom { get; set;  }


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
        /// Gets the length of the initialization vector carried explicitly in the record (usually 8 bytes).
        /// </summary>
        public static int GetRecordVectorLength(CipherAlgorithmType cipherAlgorithm, TlsSecurityParameters.TlsCipherType cipherType)
        {
            if (cipherType == TlsSecurityParameters.TlsCipherType.Stream) return 0;

            switch(cipherAlgorithm)
            {
                case CipherAlgorithmType.Rc4: return 0;
                case CipherAlgorithmType.TripleDes: return 8;
                case CipherAlgorithmType.Aes128: 
                case CipherAlgorithmType.Aes192: 
                case CipherAlgorithmType.Aes256: return 8;
                default: return 0;
            }
        }

        /// <summary>
        /// Gets the length of the fixed part of the initialization vector. This is usually computed 
        /// from the premaster key. Default is 4 bytes.
        /// </summary>
        public static int GetFixedVectorLength(CipherAlgorithmType cipherAlgorithm, TlsSecurityParameters.TlsCipherType cipherType)
        {
            if (cipherType == TlsSecurityParameters.TlsCipherType.Stream) return 0;

            switch (cipherAlgorithm)
            {
                case CipherAlgorithmType.Rc4: return 0;
                case CipherAlgorithmType.TripleDes: return 4;
                case CipherAlgorithmType.Aes128:
                case CipherAlgorithmType.Aes192:
                case CipherAlgorithmType.Aes256: return 4;
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

        public static int GetMacLength(HashAlgorithmType hashAlgorithm)
        {
            switch(hashAlgorithm)
            {
                case HashAlgorithmType.Md5: return 16;
                case HashAlgorithmType.Sha1: return 20;
                case HashAlgorithmType.Sha256: return 256 / 8;
                case HashAlgorithmType.Sha384: return 384 / 8;
                case HashAlgorithmType.Sha512: return 512 / 8;
                default: return 0;
            }
        }

        public static int GetMacKeyLength(HashAlgorithmType hashAlgorithm)
        {
            return GetMacLength(hashAlgorithm);
        }

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

        public TlsKeyBlock GetTlsKeyBlock(TlsSecurityParameters securityParameters)
        {

            // key_block = PRF(SecurityParameters.master_secret, "key expansion",
            // SecurityParameters.server_random + SecurityParameters.client_random);
            var bytes = securityParameters.PrfAlgorithm.GetSecretBytes(MasterSecret, "key expansion",
                                                                       ByteString.Combine(ServerRandom, ClientRandom),
                                                                       securityParameters.KeyMaterialSize);
                                                                       
            return new TlsKeyBlock(bytes, securityParameters.MacKeyLength, securityParameters.EncodingKeyLength, securityParameters.FixedIVLength);
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

            ccm.Init(false, new AeadParameters(key, 16 * 8, nonceBytes.ToArray(), additionalData.ToArray()));

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
    }
}
