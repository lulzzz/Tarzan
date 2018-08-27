using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using System;
using System.Collections.Generic;

namespace Tarzan.Nfx.Ingest
{

    public class TcpConversation
    {
        public KeyValuePair<PacketFlowKey, TcpStream> RequestFlow { get; set; }
        public KeyValuePair<PacketFlowKey, TcpStream> ResponseFlow { get; set; }

        internal class Comparer : IEqualityComparer<PacketFlowKey>
        {
            public bool Equals(PacketFlowKey x, PacketFlowKey y)
            {
                return PacketFlowKey.Equals(x, y) ||
                    (x.Protocol == y.Protocol
                     && x.SourcePort == y.DestinationPort
                     && y.SourcePort == x.DestinationPort
                     && x.SourceAddress.SequenceEqual(y.DestinationAddress)
                     && y.SourceAddress.SequenceEqual(x.DestinationAddress)
                    );
            }

            public int GetHashCode(PacketFlowKey obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
