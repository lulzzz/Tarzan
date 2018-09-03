using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using System;
using System.Collections.Generic;

namespace Tarzan.Nfx.Ingest
{

    public class TcpConversation
    {
        public KeyValuePair<FlowKey, TcpStream> RequestFlow { get; set; }
        public KeyValuePair<FlowKey, TcpStream> ResponseFlow { get; set; }

        /// <summary>
        /// Implementes flow matching comparer. The comparer
        /// </summary>
        internal class Comparer : IEqualityComparer<FlowKey>
        {
            public bool Equals(FlowKey x, FlowKey y)
            {
                return 
                    (x.Protocol == y.Protocol
                     && x.SourcePort == y.DestinationPort
                     && y.SourcePort == x.DestinationPort
                     && x.SourceAddress.SequenceEqual(y.DestinationAddress)
                     && y.SourceAddress.SequenceEqual(x.DestinationAddress)
                    );
            }

            // the special hash code for mapping flow keys to int:
            // P:IP1:PN1>IP2:PN2  = P:IP2:PN2>IP1:PN1
            public int GetHashCode(FlowKey obj)
            {
                var proto = (int)obj.Protocol ^ (int)obj.ProtocolFamily;
                var ip1 = obj.SourceEndpoint.GetHashCode();
                var ip2 = obj.DestinationEndpoint.GetHashCode();
                return proto ^ (ip1 ^ ip2);
            }
        }
    }
}
