using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Tarzan.Nfx.Dashboard.DataAccess;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/http")]
    public class HttpController : Controller
    {
        ITableDataAccess<HttpInfo, Guid, string> m_dataAccess;
        public HttpController(ITableDataAccess<HttpInfo, Guid,string> dataAccess)
        {
            m_dataAccess = dataAccess;
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

        [HttpGet("item/{flow-id}/{dns-id}")]
        public HttpInfo Get(string flowId, string dnsId)
        {
            var uuid = Guid.Parse(flowId);
            return m_dataAccess.FetchItem(uuid, dnsId);
        }
                
        // PUT: api/hosts/5
        [HttpPut("item/{address}")]
        public void Put(string address, [FromBody]HttpInfo value)
        {
        }        
    }
}
