using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Tarzan.Nfx.Packets.Common;

namespace TlsClassification
{

    public abstract class PrfAlgorithm
    {
        public abstract byte[] GetSecretBytes(byte[] secret, string labelString, byte[] random, int requiredLength);
        /// <summary>
        /// Gets the A values, which means: A(i) = HMAC_hash(secret, A(i-1)), where A(0) = seed.
        /// </summary>
        /// <returns>The infinite collection of A(1),A(2),....</returns>
        /// <param name="algo">Algo.</param>
        /// <param name="seed">Seed.</param>
        protected IEnumerable<byte[]> GetAValues(HashAlgorithm algo, byte[] seed)
        {
            var next = seed;
            while (true)
            {
                next = algo.ComputeHash(next);
                yield return next;
            }
        }

        public static byte[] XorBytes(int requiredLength, byte[] md5bytes, byte[] shabytes)
        {
            // trim both arrays to required length
            var resultBytes = new byte[requiredLength];

            // compute XOR: pmd5bytes ^ pshabytes
            for (var i = 0; i < requiredLength; i++)
            {
                resultBytes[i] = (byte)(md5bytes[i] ^ shabytes[i]);
            }

            return resultBytes;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }
    }

    /// <summary>
    /// Implements Prf algorithm for TLSv1.1 (https://tools.ietf.org/html/rfc4346#section-5).
    /// </summary>
    public class Tls1PrfAlgorithm : PrfAlgorithm
    {
        public override byte[] GetSecretBytes(byte[] secret, string labelString, byte[] random, int requiredLength)
        {
            var label = Encoding.ASCII.GetBytes(labelString);
            var key1 = new Span<byte>(secret).Slice(0, secret.Length / 2).ToArray();
            var key2 = new Span<byte>(secret).Slice(secret.Length / 2, secret.Length / 2).ToArray();

            var hmacMd5 = new HMACMD5(key1);
            var hmacSha = new HMACSHA1(key2);

            var md5iterations = (requiredLength / hmacMd5.HashSize) + 1;
            var shaiterations = (requiredLength / hmacSha.HashSize) + 1;

            var seed = Combine(label, random);

            var md5byteArrays = GetAValues(hmacMd5, seed)
                .Take(md5iterations)
                .Select(a => hmacMd5.ComputeHash(Combine(a, seed)));

            var md5bytes = Combine(md5byteArrays.ToArray());

            var shaByteArrays = GetAValues(hmacSha, seed)
                .Take(shaiterations)
                .Select(a => hmacSha.ComputeHash(Combine(a, seed)));

            var shabytes = Combine(shaByteArrays.ToArray());

            byte[] resultBytes = XorBytes(requiredLength, md5bytes, shabytes);

            // PRF(secret, label, random) = P_MD5(S1, label + random) XOR P_SHA1(S2, label + random);
            return resultBytes;
        }
    }

    public class TlsPrfSha256 : PrfAlgorithm
    {
        public override byte[] GetSecretBytes(byte[] secret, string labelString, byte[] random, int requiredLength)
        {
            var hmacSha256 = new HMACSHA256(secret);
            var shaiterations = (requiredLength / hmacSha256.HashSize) + 1;
            var label = Encoding.ASCII.GetBytes(labelString);
            var seed = Combine(label, random);
            var shaByteArrays = GetAValues(hmacSha256, seed)
                .Take(shaiterations)
                .Select(a => hmacSha256.ComputeHash(Combine(a, seed)));
            var shabytes = new Span<byte>(Combine(shaByteArrays.ToArray())).Slice(0, requiredLength).ToArray();
            return shabytes;
        }
    }
    public class TlsSecurityParameters
    {
        public enum TlsCipherType { Stream, Block, Aead }
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

        public int KeyMaterialSize => MacKeyLength * 2
                                    + EncodingKeyLength * 2
                                    + FixedIVLength * 2;
    }
    public class TlsKeyBlock  
    {
        private readonly byte[] bytes;
        readonly int macSize;
        readonly int keySize;
        readonly int ivSize;

        public TlsKeyBlock(byte[] bytes, int macSize, int keySize, int ivSize)
        {
            this.ivSize = ivSize;
            this.keySize = keySize;
            this.macSize = macSize;
            this.bytes = bytes;
        }
        public Span<byte> ClientWriteMacSecret => new Span<byte>(bytes).Slice(0, macSize);
        public Span<byte> ServerWriteMacSecret => new Span<byte>(bytes).Slice(macSize, macSize);
        public Span<byte> ClientWriteKey => new Span<byte>(bytes).Slice(macSize + macSize, keySize);
        public Span<byte> ServerWriteKey => new Span<byte>(bytes).Slice(macSize + macSize + keySize, keySize);
        public Span<byte> ClientIV => new Span<byte>(bytes).Slice(macSize + macSize + keySize + keySize, ivSize);
        public Span<byte> ServerIV => new Span<byte>(bytes).Slice(macSize + macSize + keySize + keySize + ivSize, ivSize);
    }


    public class TlsDecoder
    {
        public byte[] MasterSecret { get; }
        public byte[] ClientRandom { get; }
        public byte[] ServerRandom { get; }

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


        public TlsSecurityParameters GetSecurityParameters(SslProtocols version, TlsCipherSuite suite)
        {
            // TODO: get block/stream cipher name from suite:
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

        private int GetRecordVectorLength(CipherAlgorithmType cipherAlgorithm, TlsSecurityParameters.TlsCipherType cipherType)
        {
            throw new NotImplementedException();
        }

        private int GetFixedVectorLength(CipherAlgorithmType cipherAlgorithm, TlsSecurityParameters.TlsCipherType cipherType)
        {
            throw new NotImplementedException();
        }

        private PrfAlgorithm GetPrfAlgorithm(SslProtocols version)
        {
            throw new NotImplementedException();
        }

        private int GetMacLength(HashAlgorithmType hashAlgorithm)
        {
            switch(hashAlgorithm)
            {
                case HashAlgorithmType.Md5: return 128 / 8;
                case HashAlgorithmType.Sha1: return 160 / 8;
                case HashAlgorithmType.Sha256: return 256 / 8;
                case HashAlgorithmType.Sha384: return 384 / 8;
                case HashAlgorithmType.Sha512: return 512 / 8;
                default: return 0;
            }
        }

        private int GetMacKeyLength(HashAlgorithmType hashAlgorithm)
        {
            return GetMacLength(hashAlgorithm);
        }

        private int GetBlockLength(CipherAlgorithmType cipherAlgorithm)
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

        private int GetEncodingKeyLength(CipherAlgorithmType cipherAlgorithm)
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

        private HashAlgorithmType GetHashAlgorithm(string suiteString)
        {
            if (suiteString.Contains("MD5"))    return HashAlgorithmType.Md5;
            if (suiteString.Contains("SHA256")) return HashAlgorithmType.Sha256;
            if (suiteString.Contains("SHA384")) return HashAlgorithmType.Sha384;
            if (suiteString.Contains("SHA512")) return HashAlgorithmType.Sha512;
            if (suiteString.Contains("SHA"))    return HashAlgorithmType.Sha1;
            return HashAlgorithmType.None;
        }

        private CipherAlgorithmType GetCipherAlgorithm(string cipherString)
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

        private TlsSecurityParameters.TlsCipherType GetCipherType(string cipherString)
        {
            // Block (RFC5246-6.2.3.2): CBC
            if (cipherString.Contains("CBC")) return TlsSecurityParameters.TlsCipherType.Block;

            // AEAD (RFC5246-6.2.3.3): CCM, GCM
            if (cipherString.Contains("CCM")) return TlsSecurityParameters.TlsCipherType.Aead;
            if (cipherString.Contains("GCM")) return TlsSecurityParameters.TlsCipherType.Aead;

            // Stream (RFC5246-6.2.3.1): 
            return TlsSecurityParameters.TlsCipherType.Stream;
        }

        public TlsKeyBlock GetTlsKeyBlock(TlsSecurityParameters securityParameters)
        {

            // key_block = PRF(SecurityParameters.master_secret, "key expansion",
            //   SecurityParameters.server_random + SecurityParameters.client_random);
            var bytes = securityParameters.PrfAlgorithm.GetSecretBytes(MasterSecret, "key expansion",
                                                                       PrfAlgorithm.Combine(ServerRandom, ClientRandom),
                                                                       securityParameters.KeyMaterialSize);
                                                                       
            return new TlsKeyBlock(bytes, securityParameters.MacKeyLength, securityParameters.EncodingKeyLength, securityParameters.FixedIVLength);
        }
    }
}
