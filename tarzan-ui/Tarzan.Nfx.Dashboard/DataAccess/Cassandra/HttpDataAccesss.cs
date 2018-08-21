using Cassandra;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    internal class HttpDataAccesssNoContent : TableDataAccess<HttpInfo, Guid, string>
    {
        public HttpDataAccesssNoContent(ISession session) : base(session, 
            nameof(HttpInfo), 
            nameof(HttpInfo.FlowId).ToLowerInvariant(), 
            nameof(HttpInfo.TransactionId).ToLowerInvariant(),
            nameof(HttpInfo.Client),
            nameof(HttpInfo.FlowId),
            nameof(HttpInfo.Host),
            nameof(HttpInfo.Method),
            nameof(HttpInfo.RequestBodyLength),
            nameof(HttpInfo.RequestHeaders),
            nameof(HttpInfo.ResponseBodyLength),
            nameof(HttpInfo.ResponseHeaders),
            nameof(HttpInfo.Server),
            nameof(HttpInfo.StatusCode),
            nameof(HttpInfo.StatusMessage),
            nameof(HttpInfo.Timestamp),
            nameof(HttpInfo.TransactionId),
            nameof(HttpInfo.Uri),
            nameof(HttpInfo.UserAgent),
            nameof(HttpInfo.Version)
            )
        {
        }
    }
    /// <summary>
    /// Helps to distinguish between <see cref="HttpDataAccesssNoContent" and <see cref="HttpDataAccesssWithContent"/>.
    /// </summary>
    public class HttpInfoWithContent : HttpInfo { }
    internal class HttpDataAccesssWithContent : TableDataAccess<HttpInfoWithContent, Guid, string>
    {
        public HttpDataAccesssWithContent(ISession session) : base(session, nameof(HttpInfo), nameof(HttpInfo.FlowId).ToLowerInvariant(), nameof(HttpInfo.TransactionId).ToLowerInvariant())
        {
        }
    }
}