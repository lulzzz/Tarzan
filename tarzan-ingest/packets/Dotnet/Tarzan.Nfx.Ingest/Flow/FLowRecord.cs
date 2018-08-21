using Netdx.ConversationTracker;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest
{
    public static class _FlowRecord
    {
        public static bool IntersectsWith(this FlowRecord @this, FlowRecord that)
        {
            if (@this.FirstSeen <= that.FirstSeen)
            {
                return that.FirstSeen <= @this.LastSeen;
            }
            else
            {
                return @this.FirstSeen <= that.LastSeen;
            }
        }
    }
}
