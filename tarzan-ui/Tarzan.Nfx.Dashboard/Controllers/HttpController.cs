using System.Linq;
using Cassandra;
using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Tarzan.Nfx.Model;
using System;
using System.Text.RegularExpressions;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/http")]
    public class HttpController : Controller
    {
        private Table<HttpInfo> m_table;
        public HttpController(ISession session)
        {
            m_table = new Table<HttpInfo>(session);            
        }

        Regex sizeWithUnitRegex = new Regex(@"(\d+)\s*(\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Func<HttpInfo, bool> CreateFilterPredicate(HttpInfoFilter filter)
        {            
            long? getSize(string sizeString)
            {
                if(sizeString == null) return null;
                var match = sizeWithUnitRegex.Match(sizeString);
                if (match.Success)
                {
                    var size = Int64.Parse(match.Groups[1].Value);
                    var unit = match.Groups[2].Value;
                    switch (unit.ToUpperInvariant())
                    {
                        case "B": return size;
                        case "KB": return size * 1000;
                        case "MB": return size * 1000000;
                        case "GB": return size * 1000000000;
                        default: return size;
                    }
                }
                return null;
            }
            var minSize = getSize(filter.AtLeastSize);
            var maxSize = getSize(filter.AtMostSize);
            var contentList = filter.ContentTypeList;
            return (HttpInfo x) =>
             {
                 var url = (x.Host ?? String.Empty) + (x.Uri ?? String.Empty);                  
                 if (!String.IsNullOrWhiteSpace(filter.Uri) && !url.Contains(filter.Uri)) return false;
                 if (contentList != null && !contentList.Contains(x.ResponseContentType, StringComparer.InvariantCultureIgnoreCase)) return false;
                 if (x.ResponseBodyLength < minSize) return false;
                 if (x.ResponseBodyLength > maxSize) return false;
                 return true;
             };
        }

        [HttpGet("count")]
        public int GetCount([FromQuery]HttpInfoFilter filter)
        {
            return (from row in m_table
             select new HttpInfo()
             {
                 FlowId = row.FlowId,
                 TransactionId = row.TransactionId,
                 Host = row.Host,
                 Uri = row.Uri,
                 ResponseBodyLength = row.ResponseBodyLength,
                 ResponseContentType = row.ResponseContentType,
             }).Execute().Where(CreateFilterPredicate(filter)).Count();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<HttpInfo> Get(int start, int length, [FromQuery]HttpInfoFilter filter)
        {
            var source = (from row in m_table select new HttpInfo()
            {
                FlowId = row.FlowId,
                TransactionId = row.TransactionId,
                Client = row.Client,
                Server = row.Server,
                Timestamp = row.Timestamp,
                Method = row.Method,
                Host = row.Host,
                Uri = row.Uri,
                Version = row.Version,
                StatusCode = row.StatusCode,
                StatusMessage = row.StatusMessage,
                RequestBodyLength = row.RequestBodyLength,
                RequestHeaders = row.RequestHeaders,
                RequestContentType = row.RequestContentType,
                ResponseBodyLength = row.ResponseBodyLength,                
                ResponseHeaders = row.ResponseHeaders, 
                ResponseContentType = row.ResponseContentType,

            }).Execute();
            var filteredSource = source.Where(CreateFilterPredicate(filter));
            return filteredSource.OrderBy(x=>x.Timestamp).Skip(start).Take(length);
        }

        [HttpGet("item/{flowId}/{transactionId}")]
        public HttpInfo Get(string flowId, string transactionId)
        {
            return (from row in m_table where row.FlowId == flowId && row.TransactionId == transactionId select row).FirstOrDefault().Execute();
        }                    
    }
}
