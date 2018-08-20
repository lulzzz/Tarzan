using Cassandra;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    internal class HttpDataAccesss : TableDataAccess<HttpInfo, Guid, string>
    {
        public HttpDataAccesss(ISession session) : base(session, nameof(HttpInfo), nameof(HttpInfo.FlowId).ToLowerInvariant(), nameof(HttpInfo.TransactionId).ToLowerInvariant())
        {
        }
    }
}