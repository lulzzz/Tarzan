using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;
namespace Tarzan.Nfx.Model.Tests
{
    /// <summary>
    /// Implements benchmarks for various hash functions for key flow.
    /// The key flow has a fixed length of 40 bytes. We need a hash function
    /// that has a good properties and is really fast.
    /// </summary>
    public class FlowKeyHashBenchmark
    {
        byte[] flowKeyBytes;
        
        HashAlgorithm murmurHash;
        [GlobalSetup]
        public void Setup()
        {
            Random random = new Random();
            flowKeyBytes = new byte[40];
            random.NextBytes(flowKeyBytes);
            murmurHash = Murmur.MurmurHash.Create32();
        }


        [Benchmark]
        public unsafe int JavaByteArrayHashFixed()
        {
            fixed (byte* bytePtr = flowKeyBytes)
            {
                var intPtr = (int*)bytePtr;
                return (31 * (31 * (31 * (31 * (31 * (31 * (31 * (31 * (31 * intPtr[0] + intPtr[1]) + intPtr[2]) + intPtr[3]) + intPtr[4])
                    + intPtr[5]) + intPtr[6]) + intPtr[7]) + intPtr[8]) + intPtr[9]);
            }
        }

        [Benchmark]
        public int JavaByteArrayHash()
        {
            var result = 1;
            for(int i =0; i < flowKeyBytes.Length; i++)
                result = 31 * result + flowKeyBytes[i];
            return result;
        }

        [Benchmark]
         public unsafe int XorHashFixed()
        {
            fixed (byte* bytePtr = flowKeyBytes)
            {
                var intPtr = (int*)bytePtr;
                return intPtr[0] ^ intPtr[1] ^ intPtr[2] ^ intPtr[3] ^ intPtr[4]
                    ^ intPtr[5] ^ intPtr[6] ^ intPtr[7] ^ intPtr[8] ^ intPtr[9];
            }    
        }
        [Benchmark]
        public int MurmurHash()
        {
            var bytes = murmurHash.ComputeHash(flowKeyBytes); 
            return BitConverter.ToInt32(bytes,0);   
        }
    }
}