using System;
using System.Net;

namespace Tarzan.Nfx.Ingest.Flow
{
    public class FlowUidGenerator
    {
        static Murmur.Murmur128 hashAlgorithm = Murmur.MurmurHash.Create128();
        // The requirements for these types of UUIDs are as follows:
        //
        // The UUIDs generated at different times from the same name in the
        // same namespace MUST be equal.
        // The UUIDs generated from two different names in the same namespace
        // should be different(with very high probability).
        // The UUIDs generated from the same name in two different namespaces
        // should be different with(very high probability).
        // If two UUIDs that were generated from names are equal, then they
        // were generated from the same name in the same namespace (with very
        // high probability).
        public static string NewUid(System.Net.Sockets.ProtocolType protocol, IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint, long firstSeen)
        {
            
            var protoHash = protocol.GetHashCode();
            var dstHash = destinationEndpoint.GetHashCode() ^ protoHash;
            var srcHash = sourceEndpoint.GetHashCode() ^ protoHash;
            var keyHash = srcHash ^ dstHash;
            var seenHash = firstSeen.GetHashCode();
            return $"{keyHash.ToString("X8")}-{seenHash.ToString("X8")}";
            /*
            var protoHash = protocol.GetHashCode();
            var dstHash = BitConverter.GetBytes(destinationEndpoint.GetHashCode() ^ protoHash);
            var srcHash = BitConverter.GetBytes(sourceEndpoint.GetHashCode() ^ protoHash);
            var timeHash = BitConverter.GetBytes(firstSeen);
            var buffer = new byte[16];
            timeHash.CopyTo(buffer, 0);
            srcHash.CopyTo(buffer, 8);
            dstHash.CopyTo(buffer, 12);
            var guid = new Guid(buffer);
            return guid;
            */
        }

        public static string NewUid(FlowKey flowKey, long firstSeen)
        {          
            var hashValue = hashAlgorithm.ComputeHash(flowKey.Bytes);
            var lopart = BitConverter.ToUInt64(hashValue, 0);
            var hipart = BitConverter.ToUInt64(hashValue, sizeof(ulong));
            return $"{lopart.ToString("X16")}-{hipart.ToString("X16")}";    
        }
    }
}
