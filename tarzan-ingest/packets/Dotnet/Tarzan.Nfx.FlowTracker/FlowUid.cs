using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.FlowTracker
{
    public class FlowUidGenerator
    {
        static Murmur.Murmur128 hashAlgorithm = Murmur.MurmurHash.Create128();
        public static string NewUid(FlowKey flowKey, long firstSeen)
        {          
            var hashValue = hashAlgorithm.ComputeHash(flowKey.Bytes);
            var lopart = BitConverter.ToUInt64(hashValue, 0);
            var hipart = BitConverter.ToUInt64(hashValue, sizeof(ulong));
            return $"{lopart.ToString("X16")}-{hipart.ToString("X16")}";    
        }
    }
}
