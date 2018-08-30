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

        internal class Comparer : IEqualityComparer<FlowKey>
        {
            public bool Equals(FlowKey x, FlowKey y)
            {
                return FlowKey.Equals(x, y) ||
                    (x.Protocol == y.Protocol
                     && x.SourcePort == y.DestinationPort
                     && y.SourcePort == x.DestinationPort
                     && x.SourceAddress.SequenceEqual(y.DestinationAddress)
                     && y.SourceAddress.SequenceEqual(x.DestinationAddress)
                    );
            }

            public int GetHashCode(FlowKey obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
