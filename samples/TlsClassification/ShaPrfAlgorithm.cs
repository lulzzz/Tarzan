using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    /// <summary>
    /// Implements PRF algorithm for TLSv1.2. (https://tools.ietf.org/html/rfc5246#section-5)
    /// </summary>
    /// <remarks>
    /// The PRF is a pseudo-random function computed using specified keyed hash
    /// (by default it uses SHA256):
    /// 
    /// PRF(secret, label, random) = P_HASH(secret, label + random);
    /// 
    /// where sizes of S1 and S2 are equal.
    /// 
    ///
    /// P_HASH(secret, seed) = HMAC_hash(secret, A(1) + seed) +
    ///                        HMAC_hash(secret, A(2) + seed) +
    ///                        HMAC_hash(secret, A(3) + seed) + ...
    /// 
    /// A(0) = seed
    /// A(i) = HMAC_hash(secret, A(i-1)) 
    /// 
    /// where HMAC_hash is specified hash algorithm
    /// </remarks>
    public class ShaPrfAlgorithm : PrfAlgorithm
    {
        private readonly KeyedHashAlgorithm hashAlgorithm;

        public ShaPrfAlgorithm()
        {
            this.hashAlgorithm = new HMACSHA256();
        }

        public ShaPrfAlgorithm(string hashAlgorithmType)
        {
            switch(hashAlgorithmType)
            {
                case "SHA":
                case "SHA1":
                    this.hashAlgorithm = new HMACSHA1();
                    break;

                case "SHA384":
                    this.hashAlgorithm = new HMACSHA384();
                    break;
                case "SHA512":
                    this.hashAlgorithm = new HMACSHA512();
                    break;
                default:
                    this.hashAlgorithm = new HMACSHA256();
                    break;
            }
        }
        public ShaPrfAlgorithm(KeyedHashAlgorithm hashAlgorithm)
        {
            this.hashAlgorithm = hashAlgorithm;
        }

        public override byte[] GetSecretBytes(byte[] secret, string labelString, byte[] random, int requiredLength)
        {
            var keyedHashAlgorithm = this.hashAlgorithm;
            keyedHashAlgorithm.Key = secret;
            keyedHashAlgorithm.Initialize();
            var shaiterations = (requiredLength / (keyedHashAlgorithm.HashSize/8)) + 1;
            var label = Encoding.ASCII.GetBytes(labelString);
            var seed = ByteString.Combine(label, random);
            var shaByteArrays = GetAValues(keyedHashAlgorithm, seed)
                .Take(shaiterations)
                .Select(a => keyedHashAlgorithm.ComputeHash(ByteString.Combine(a, seed)));
            var prfbytes = new Span<byte>(ByteString.Combine(shaByteArrays.ToArray())).Slice(0, requiredLength).ToArray();
            return prfbytes;
        }

        /// <summary>
        /// Tests the PRF implementation (see https://www.ietf.org/mail-archive/web/tls/current/msg03416.html).
        /// </summary>
        public static void Test100()
        {
            var secret = "9b be 43 6b a9 40 f0 17 b1 76 52 84 9a 71 db 35".Replace(" ", "");
            var seed   = "a0 ba 9f 93 6c da 31 18 27 a6 f7 96 ff d5 19 8c".Replace(" ", "");
            var label  = "test label";

            var expectedPrefix = ("e3 f2 29 ba 72 7b e1 7b").Replace(" ","");
            var prf = new ShaPrfAlgorithm();
            var output = prf.GetSecretBytes(ByteString.StringToByteArray(secret), label, ByteString.StringToByteArray(seed), 100);
            var outputString = ByteString.ByteArrayToString(output);
            var isSame = outputString.StartsWith(expectedPrefix, StringComparison.InvariantCulture);
            if (!isSame) throw new InvalidOperationException("Something is wrong with PRF algorithm!");
        }
    }
}
