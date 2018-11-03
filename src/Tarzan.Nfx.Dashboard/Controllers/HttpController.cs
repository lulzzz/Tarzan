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
        private IAffDataset m_dataset;
        public HttpController(IAffDataset dataset)
        {
            m_dataset = dataset;            
        }

        Regex sizeWithUnitRegex = new Regex(@"(\d+)\s*(\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Func<HttpObject, bool> CreateFilterPredicate(HttpInfoFilter filter)
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
            return (HttpObject x) =>
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
            return (from row in m_dataset.HttpTable
             select new HttpObject()
             {
                 FlowUid = row.FlowUid,
                 ObjectIndex = row.ObjectIndex,
                 Host = row.Host,
                 Uri = row.Uri,
                 ResponseBodyLength = row.ResponseBodyLength,
                 ResponseContentType = row.ResponseContentType,
             }).Execute().Where(CreateFilterPredicate(filter)).Count();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<HttpObject> Get(int start, int length, [FromQuery]HttpInfoFilter filter)
        {
            var source = (from row in m_dataset.HttpTable select new HttpObject()
            {
                FlowUid = row.FlowUid,
                ObjectIndex = row.ObjectIndex,
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

        [HttpGet("item/{flowUId}/{objectIndex}")]
        public HttpObject Get(string flowUId, string objectIndex)
        {
            return m_dataset.HttpTable.Where(x => x.FlowUid == flowUId && x.ObjectIndex == objectIndex).FirstOrDefault().Execute();
        }                    
    }
}
