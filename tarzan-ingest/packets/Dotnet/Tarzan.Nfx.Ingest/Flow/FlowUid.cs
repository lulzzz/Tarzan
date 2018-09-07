using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest.Flow
{
    public class FlowUid
    {
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
        public static Guid NewUid(System.Net.Sockets.ProtocolType protocol, IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint, long firstSeen)
        {
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
        }

        public static Guid NewUid(FlowKey flowKey, long firstSeen)
        {
            return NewUid(flowKey.Protocol, flowKey.SourceEndpoint, flowKey.DestinationEndpoint, firstSeen);    
        }

        public static Guid NewUid(System.Net.Sockets.ProtocolType protocol, byte[] sourceAddressBytes, int sourcePort, byte[] destinationAddressBytes, int destinationPort, long firstSeen)
        {
            return NewUid(protocol, new IPEndPoint(new System.Net.IPAddress(sourceAddressBytes), sourcePort), new IPEndPoint(new System.Net.IPAddress(destinationAddressBytes), destinationPort), firstSeen);
        }
    }
}
