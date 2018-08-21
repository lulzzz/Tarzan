using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Tarzan.Nfx.Dashboard.DataAccess;
using Tarzan.Nfx.Dashboard.DataAccess.Cassandra;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/http")]
    public class HttpController : Controller
    {
        ITableDataAccess<HttpInfo, Guid, string> m_dataAccess;
        ITableDataAccess<HttpInfoWithContent, Guid, string> m_fullDataAccess;
        public HttpController(ITableDataAccess<HttpInfo, Guid,string> dataAccess, ITableDataAccess<HttpInfoWithContent, Guid, string> fullDataAccess)
        {
            m_dataAccess = dataAccess;
            m_fullDataAccess = fullDataAccess;
        }

        [HttpGet("count")]
        public int GetCount()
        {
            return m_dataAccess.Count();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<HttpInfo> Get(int start, int length)
        {
            return m_dataAccess.FetchRange(start, length); 
        }

        [HttpGet("item/{flowId}/{transactionId}")]
        public HttpInfo Get(string flowId, string transactionId)
        {
            var uuid = Guid.Parse(flowId);
            return m_fullDataAccess.FetchItem(uuid, transactionId);
        }
                
        // PUT: api/hosts/5
        [HttpPut("item/{address}")]
        public void Put(string address, [FromBody]HttpInfo value)
        {
        }        
    }
}
