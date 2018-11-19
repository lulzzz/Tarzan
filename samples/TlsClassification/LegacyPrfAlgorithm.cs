using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    /// <summary>
    /// Implements PRF algorithm for TLSv1.1 (https://tools.ietf.org/html/rfc4346#section-5).
    /// </summary>
    /// <remarks>
    /// The PRF is a pseudo-random function computed using MD5 and SHA1 hashes:
    /// 
    /// PRF(S1 + S2, label, random) = P_MD5(S1, label + random) XOR P_SHA1(S2, label + random);
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
    /// where HMAC_hash is MD5 or SHA1, respectively.
    /// </remarks>
    public class LegacyPrfAlgorithm : PrfAlgorithm
    {
        public override byte[] GetSecretBytes(byte[] secret, string labelString, byte[] random, int requiredLength)
        {
            var label = Encoding.ASCII.GetBytes(labelString);
            var key1 = new Span<byte>(secret).Slice(0, secret.Length / 2).ToArray();
            var key2 = new Span<byte>(secret).Slice(secret.Length / 2, secret.Length / 2).ToArray();

            var hmacMd5 = new HMACMD5(key1);
            var hmacSha = new HMACSHA1(key2);

            var md5iterations = (requiredLength / (hmacMd5.HashSize/8)) + 1;
            var shaiterations = (requiredLength / (hmacSha.HashSize/8)) + 1;

            var seed = ByteString.Combine(label, random);

            var md5byteArrays = GetAValues(hmacMd5, seed)
                .Take(md5iterations)
                .Select(a => hmacMd5.ComputeHash(ByteString.Combine(a, seed)));

            var md5bytes = ByteString.Combine(md5byteArrays.ToArray());

            var shaByteArrays = GetAValues(hmacSha, seed)
                .Take(shaiterations)
                .Select(a => hmacSha.ComputeHash(ByteString.Combine(a, seed)));

            var shabytes = ByteString.Combine(shaByteArrays.ToArray());

            byte[] prfbytes = XorBytes(requiredLength, md5bytes, shabytes);

            // 
            return prfbytes;
        }
    }
}
