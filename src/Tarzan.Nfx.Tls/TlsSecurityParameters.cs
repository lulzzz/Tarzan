using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto.Engines;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Tls
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
    public enum TlsCipherMode { Unknown, CBC, EDE_CBC, CCM, CCM_8,  GCM }

    public class TlsSecurityParameters
    {
        public string CipherAlgorithm { get; set; }
        public TlsCipherMode CipherMode { get; set; }
        public TlsCipherType CipherType
        {
            get
            {
                if (IsStreamAlgorithm(this.CipherAlgorithm)) return TlsCipherType.Stream;
                switch (CipherMode)
                {
                    case TlsCipherMode.EDE_CBC:
                    case TlsCipherMode.CBC: return TlsCipherType.Block;
                    case TlsCipherMode.CCM:
                    case TlsCipherMode.CCM_8:
                    case TlsCipherMode.GCM: return TlsCipherType.Aead;
                }
                return TlsCipherType.Unknown;
            }
        }

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
            var sp = new TlsSecurityParameters();
            var cipherSuiteName = new TlsCipherSuiteName(cipherSuite);

            sp.ProtocolVersion = protocolVersion;
            sp.CipherAlgorithm = cipherSuiteName.BlockCipherName;
            sp.CommpressionMethod = compressionMethod;
            sp.EncodingKeyLength = GetEncodingKeyLength(cipherSuiteName.BlockCipherName, cipherSuiteName.BlockCipherSize);

            if (IsStreamAlgorithm(cipherSuiteName.BlockCipherName))
            {
                sp.CipherMode = TlsCipherMode.Unknown;
                SetStreamCipher(cipherSuiteName, sp);
            }
            else
            {
                var cipherMode = TlsCipherMode.Unknown;
                Enum.TryParse(cipherSuiteName.BlockCipherMode, true, out cipherMode);
                sp.CipherMode = cipherMode;
                SetBlockCipher(cipherSuiteName, sp);
            }
            return sp;
        }

        private static void SetStreamCipher(TlsCipherSuiteName cipherSuiteName, TlsSecurityParameters sp)
        {
            throw new NotImplementedException();
        }

        private static void SetBlockCipher(TlsCipherSuiteName cipherSuiteName, TlsSecurityParameters sp)
        {
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
                case TlsCipherMode.EDE_CBC:
                case TlsCipherMode.CBC:
                    {
                        sp.MacAlgorithm = cipherSuiteName.MacAlgorithm;
                        sp.MacKeyLength = GetMacLength(cipherSuiteName.MacAlgorithm); // actually the same value as mac length
                        sp.MacLength = GetMacLength(cipherSuiteName.MacAlgorithm);
                        sp.FixedIVLength = GetBlockLength(cipherSuiteName.BlockCipherName);
                        sp.RecordIVLength = 0;
                        sp.PrfHashAlgorithm = cipherSuiteName.MacAlgorithm;
                        break;
                    }
            }
        }

        private static bool IsStreamAlgorithm(string cipherSuiteName)
        {
            if (string.Equals(cipherSuiteName, "RC4"))
                return true;
            else
                return false;
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
                case "IDEA": return 64;
                default: return 128;
            }
        }

        /// <summary>
        /// Gets the length in bits of the encoding key for cipher algorithm.
        /// </summary>
        /// <returns>The encoding key length.</returns>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        public static int GetEncodingKeyLength(string cipherAlgorithm, string length)
        {
            if (length!=null && Int32.TryParse(length, out int keyLength))
            {
                return keyLength;
            }
            // length not explicitly specified:
            switch (cipherAlgorithm.ToUpperInvariant())
            {
                case "AES": return 128;
                case "DES": return 64;
                case "3DES": return 192;
                case "RC2": return 64;
                case "RC4": return 128;
                case "IDEA": return 128;
                case "CAMELLIA" : return 128;
                case "SEED": 
                default: return 128;
            }
        }

        public static SslProtocols GetSslProtocolVersion(int major, int minor)
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
