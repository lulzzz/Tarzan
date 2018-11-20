using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public class TlsSecurityParameters
    {
        public enum TlsCipherType { Stream, Block_CBC, Aead_CCM, Aead_CCM_8, Aead_GCM }
        public enum TlsCommpressionMethod { Null }

        public PrfAlgorithm PrfAlgorithm { get; set; }
        public CipherAlgorithmType CipherAlgorithm { get; set; }
        public TlsCipherType CipherType { get; set; }
        public int EncodingKeyLength { get; set; }
        public int BlockLength { get; set; }
        public int FixedIVLength { get; set; }
        public int RecordIVLength { get; set; }

        public HashAlgorithmType MacAlgorithm { get; set; }
        public int MacLength { get; set; }
        public int MacKeyLength { get; set; }

        public TlsCommpressionMethod CommpressionMethod { get; set; }

        /// <summary>
        /// Gets the size in bits of the key material.
        /// </summary>
        /// <value>The size of the key material.</value>
        public int KeyMaterialSize => MacKeyLength * 2
                                    + EncodingKeyLength * 2
                                    + FixedIVLength * 2;


        public static TlsSecurityParameters Create(SslProtocols protocolVersion, string cipherSuite)
        {
            var cipherAlgorithm = GetCipherAlgorithm(cipherSuite);
            var hashAlgorithm = GetHashAlgorithm(cipherSuite);
            var cipherType = GetCipherType(cipherSuite);
            var prfAlgorithm = protocolVersion == SslProtocols.Tls12 ? new ShaPrfAlgorithm(hashAlgorithm) as PrfAlgorithm: new LegacyPrfAlgorithm() as PrfAlgorithm;
            return CreateSecurityParameters(cipherAlgorithm, cipherType, hashAlgorithm, prfAlgorithm);
        }

        private static TlsSecurityParameters CreateSecurityParameters(CipherAlgorithmType cipherAlgorithm, TlsCipherType cipherType, HashAlgorithmType hashAlgorithm, PrfAlgorithm prfHashAlgorithm)
        {
            var sp = new TlsSecurityParameters();
            sp.CipherAlgorithm = cipherAlgorithm;
            sp.CipherType = cipherType;
            sp.BlockLength = GetBlockLength(cipherAlgorithm);
            sp.CommpressionMethod = TlsCommpressionMethod.Null;
            sp.EncodingKeyLength = GetEncodingKeyLength(cipherAlgorithm);
            sp.PrfAlgorithm = prfHashAlgorithm;

            switch (cipherType)
            {
                case TlsCipherType.Aead_CCM:
                case TlsCipherType.Aead_GCM:
                    {
                        sp.MacAlgorithm = HashAlgorithmType.None;
                        sp.MacKeyLength = 0;
                        sp.MacLength = 16 * 8;
                        sp.FixedIVLength = 4 * 8;
                        sp.RecordIVLength = 8 * 8;
                        break;
                    }
                case TlsCipherType.Aead_CCM_8:
                    {
                        sp.MacAlgorithm = HashAlgorithmType.None;
                        sp.MacKeyLength = 0;
                        sp.MacLength = 8 * 8;
                        sp.FixedIVLength = 4 * 8;
                        sp.RecordIVLength = 8 * 8;
                        break;
                    }
                case TlsCipherType.Block_CBC:
                    {
                        sp.MacAlgorithm = hashAlgorithm;
                        sp.MacKeyLength = GetMacKeyLength(hashAlgorithm);
                        sp.MacLength = GetMacLength(hashAlgorithm);

                        throw new NotImplementedException();
                        //sp.FixedIVLength = 4 * 8;
                        //sp.RecordIVLength = 8 * 8;
                        break;
                    }
            }
            return sp;
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
        public static TlsCipherType GetCipherType(string cipherString)
        {
            // Block (RFC5246-6.2.3.2): CBC
            if (cipherString.Contains("CBC")) return TlsCipherType.Block_CBC;

            // AEAD (RFC5246-6.2.3.3): CCM, GCM
            if (cipherString.Contains("CCM_8")) return TlsCipherType.Aead_CCM_8;
            if (cipherString.Contains("CCM")) return TlsCipherType.Aead_CCM;
            if (cipherString.Contains("GCM")) return TlsCipherType.Aead_GCM;

            // Stream (RFC5246-6.2.3.1): 
            return TlsCipherType.Stream;
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
                CommpressionMethod = TlsCommpressionMethod.Null,
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

            switch (cipherAlgorithm)
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
            switch (hashAlgorithm)
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
            switch (cipherAlgorithm)
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
            if (suiteString.Contains("MD5")) return HashAlgorithmType.Md5;
            if (suiteString.Contains("SHA256")) return HashAlgorithmType.Sha256;
            if (suiteString.Contains("SHA384")) return HashAlgorithmType.Sha384;
            if (suiteString.Contains("SHA512")) return HashAlgorithmType.Sha512;
            if (suiteString.Contains("SHA")) return HashAlgorithmType.Sha1;
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
            switch (major)
            {
                case 2: return SslProtocols.Ssl2;
                case 3:
                    switch (minor)
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
    }
}
