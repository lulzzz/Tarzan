using Netdx.ConversationTracker;
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
                         && x.SourceEndpoint.Equals(y.DestinationEndpoint)
                         && y.SourceEndpoint.Equals(x.DestinationEndpoint)
                        );
                }

                public int GetHashCode(FlowKey obj)
                {
                    return obj.Protocol.GetHashCode() ^ obj.DestinationEndpoint.GetHashCode() ^ obj.SourceEndpoint.GetHashCode();
                }
            }
        }
}
