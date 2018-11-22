using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto.Engines;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Samples.TlsClassification
{


    /// <summary>
    /// Helps to get information from the Tls cipher suite name.
    /// (https://testssl.sh/openssl-iana.mapping.html).
    /// </summary>
    public class TlsCipherSuiteName 
    {
        /// <summary>
        /// Regular expression that parses the cipher suite name into the three parts:
        /// Key exchange algorithm
        /// block cipher
        /// message authentication algorithm
        /// </summary>
        static Regex rxCipherSuite = new Regex("TLS_(\\w+)_WITH_(\\w+)_(\\w+)$");
        static Regex rxBlockCipher = new Regex(("^([^_]+)(_(\\d+))?(_(\\w+))?$"));

        Match cipherSuiteMatch;
        Match blockCipherMatch;

        public TlsCipherSuiteName(string cipherSuite)
        {
            cipherSuiteMatch = rxCipherSuite.Match(cipherSuite);
            blockCipherMatch = rxBlockCipher.Match(BlockCipher);
        }

        public string KeyExchange => cipherSuiteMatch.Groups[1].Value;
        public string BlockCipher => cipherSuiteMatch.Groups[2].Value;
        public string MacAlgorithm => cipherSuiteMatch.Groups[3].Value;
        public string BlockCipherName => blockCipherMatch.Groups[1].Value;
        public string BlockCipherSize => blockCipherMatch.Groups[3].Value;
        public string BlockCipherMode => blockCipherMatch.Groups[5].Value;

        public override string ToString()
        {
            return $"TLS_{KeyExchange}_WITH_{BlockCipher}_{MacAlgorithm}";
        }

        public static void Test()
        {
            foreach(var cipherSuite in Enum.GetNames(typeof(TlsCipherSuite)))
            {
                var cipherSuiteName = new TlsCipherSuiteName(cipherSuite);
                Console.WriteLine($"{cipherSuite}: {cipherSuiteName.KeyExchange} {cipherSuiteName.BlockCipherName}[{cipherSuiteName.BlockCipherSize}]({cipherSuiteName.BlockCipherMode}) {cipherSuiteName.MacAlgorithm}");
            }
        }
    }

    public enum TlsCipherType { Unknown, Stream, Block, Aead }
    public enum TlsCipherMode { Unknown, CBC, CCM, CCM_8,  GCM }

    public class TlsSecurityParameters
    {
        public string CipherAlgorithm { get; set; }
        public TlsCipherType CipherType
        {
            get
            {
                switch (CipherMode)
                {
                    case TlsCipherMode.CBC: return TlsCipherType.Block;
                    case TlsCipherMode.CCM:
                    case TlsCipherMode.CCM_8:
                    case TlsCipherMode.GCM: return TlsCipherType.Aead;
                }
                return TlsCipherType.Unknown;
            }
        }

        public TlsCipherMode CipherMode { get; set; }

        public int EncodingKeyLength { get; set; }

        public int FixedIVLength { get; set; }
        public int RecordIVLength { get; set; }

        public string MacAlgorithm { get; set; }
        public int MacLength { get; set; }
        public int MacKeyLength { get; set; }

        public string PrfHashAlgorithm { get; set; }

        SslProtocols ProtocolVersion { get; set; }

        public TlsPacket.CompressionMethods CommpressionMethod { get; set; }

        /// <summary>
        /// Gets the size in bits of the key material.
        /// </summary>
        /// <value>The size of the key material.</value>
        public int KeyMaterialSize => MacKeyLength * 2
                                    + EncodingKeyLength * 2
                                    + FixedIVLength * 2;

        public PrfAlgorithm Prf
        {
            get
            {
                switch (this.ProtocolVersion)
                {
                    case SslProtocols.Tls12:
                        return new ShaPrfAlgorithm(this.PrfHashAlgorithm);
                    default:
                        return new LegacyPrfAlgorithm();
                }
            }
        }

        public static TlsSecurityParameters Create(SslProtocols protocolVersion, string cipherSuite, TlsPacket.CompressionMethods compressionMethod = TlsPacket.CompressionMethods.NullCompression)
        {
            var cipherSuiteName = new TlsCipherSuiteName(cipherSuite);
            var sp = new TlsSecurityParameters();
            sp.ProtocolVersion = protocolVersion;
            sp.CipherAlgorithm = cipherSuiteName.BlockCipherName;
            sp.CipherMode = Enum.Parse<TlsCipherMode>(cipherSuiteName.BlockCipherMode, true);
            sp.MacAlgorithm = String.Empty;

            sp.CommpressionMethod = compressionMethod;
             if (String.IsNullOrEmpty(cipherSuiteName.BlockCipherSize))
                sp.EncodingKeyLength = GetEncodingKeyLength(cipherSuiteName.BlockCipherName);
            else
                sp.EncodingKeyLength = Int32.Parse(cipherSuiteName.BlockCipherSize);

            switch (sp.CipherMode)
            {
                case TlsCipherMode.CCM:
                case TlsCipherMode.GCM:
                    {
                        sp.MacAlgorithm = String.Empty;
                        sp.PrfHashAlgorithm = cipherSuiteName.MacAlgorithm;
                        sp.MacKeyLength = 0;
                        sp.MacLength = 16 * 8;
                        sp.FixedIVLength = 4 * 8;
                        sp.RecordIVLength = 8 * 8;
                        break;
                    }
                case TlsCipherMode.CCM_8:
                    {
                        sp.MacAlgorithm = String.Empty;
                        sp.PrfHashAlgorithm = cipherSuiteName.MacAlgorithm;
                        sp.MacKeyLength = 0;
                        sp.MacLength = 8 * 8;
                        sp.FixedIVLength = 4 * 8;
                        sp.RecordIVLength = 8 * 8;
                        break;
                    }
                case TlsCipherMode.CBC:
                    {
                        sp.MacAlgorithm = cipherSuiteName.MacAlgorithm;
                        sp.MacKeyLength = GetMacLength(cipherSuiteName.MacAlgorithm); // actually the same value as mac length
                        sp.MacLength = GetMacLength(cipherSuiteName.MacAlgorithm);
                        sp.FixedIVLength = GetBlockLength(cipherSuiteName.BlockCipher);  
                        sp.RecordIVLength = 0;
                        sp.PrfHashAlgorithm = cipherSuiteName.MacAlgorithm;
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
        public static TlsCipherMode GetCipherMode(string cipherString)
        {
            // Block (RFC5246-6.2.3.2): CBC
            if (cipherString.Contains("CBC")) return TlsCipherMode.CBC;

            // AEAD (RFC5246-6.2.3.3): CCM, GCM
            if (cipherString.Contains("CCM_8")) return TlsCipherMode.CCM_8;
            if (cipherString.Contains("CCM")) return TlsCipherMode.CCM;
            if (cipherString.Contains("GCM")) return TlsCipherMode.GCM;

            // Stream (RFC5246-6.2.3.1): 
            return TlsCipherMode.Unknown;
        }

        /// <summary>
        /// Gets the length in bits of the initialization vector carried explicitly in the record (usually 8 bytes which is 64bits).
        /// </summary>
        public static int GetRecordVectorLength(CipherAlgorithmType cipherAlgorithm, TlsCipherType cipherType)
        {
            if (cipherType == TlsCipherType.Stream) return 0;

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
        public static int GetFixedVectorLength(CipherAlgorithmType cipherAlgorithm, TlsCipherType cipherType)
        {
            if (cipherType == TlsCipherType.Stream) return 0;

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
        public static int GetMacLength(string hashAlgorithm)
        {
            switch (hashAlgorithm.ToUpperInvariant())
            {
                case "MD5": return 128;
                case "SHA":
                case "SHA1": return 160;
                case "SHA256": return 256;
                case "SHA384": return 384;
                case "SHA512": return 512;
                default: return 0;
            }
        }

        /// <summary>
        /// Gets the length in bits of the block cipher. For stream 
        /// cipher the value is equal to 1 (case of RC4).
        /// </summary>
        /// <returns>The block length.</returns>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        public static int GetBlockLength(string cipherAlgorithm)
        {
            switch (cipherAlgorithm.ToUpperInvariant())
            {
                case "AES": return 128;
                case "CAMELLIA": return 128;
                case "DES": return 64;
                case "RC2": return 64;
                case "3DES": return 64;
                case "RC4": return 1;
                case "IDEA": return 64;
                default: return 128;
            }
        }

        /// <summary>
        /// Gets the length in bits of the encoding key for cipher algorithm.
        /// </summary>
        /// <returns>The encoding key length.</returns>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        public static int GetEncodingKeyLength(string cipherAlgorithm)
        {
            switch (cipherAlgorithm.ToUpperInvariant())
            {
                case "AES128": return 128;
                case "AES192": return 192;
                case "AES256": return 256;
                case "DES": return 64;
                case "3DES": return 192;
                case "RC2": return 64;
                case "RC4": return 128;
                case "IDEA": return 128;
                case "CAMELLIA" : return 128;
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
