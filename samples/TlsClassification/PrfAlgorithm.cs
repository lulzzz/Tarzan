using System.Collections.Generic;
using System.Security.Cryptography;

namespace Tarzan.Nfx.Samples.TlsClassification
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
    }
}
