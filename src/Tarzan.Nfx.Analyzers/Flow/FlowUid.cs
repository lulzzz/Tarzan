using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers
{
    /// <summary>
    /// Generator of a unique flow IDs. It uses Murmur128 hash algorithm. The resulting 
    /// id has form of string representing 128 bit integer in hexadecimal representation.
    /// </summary>
    public class FlowUidGenerator
    {
        static Murmur.Murmur128 m_hashAlgorithm = Murmur.MurmurHash.Create128();
        /// <summary>
        /// Generates a new flow id. The id depends on flow key and first seen timestamp. 
        /// </summary>
        /// <param name="flowKey">The key of flow.</param>
        /// <param name="firstSeen">Timestemp given as UNIX milliseconds.</param>
        /// <returns>A string representation of flow ID. </returns>
        public static string NewUid(FlowKey flowKey, long firstSeen)
        {          
            var hashValue = m_hashAlgorithm.ComputeHash(flowKey.Bytes);
            var lopart = BitConverter.ToUInt64(hashValue, 0);
            var hipart = BitConverter.ToUInt64(hashValue, sizeof(ulong));
            return $"{lopart.ToString("X16")}-{hipart.ToString("X16")}";    
        }
    }
}
