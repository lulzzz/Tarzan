using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Tarzan.Nfx.Dashboard.DataAccess;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        ITableDataAccess<Dns, Guid, string> m_dataAccess;
        public DnsController(ITableDataAccess<Dns, Guid,string> dataAccess)
        {
            m_dataAccess = dataAccess;
        }

        [HttpGet("count")]
        public int GetCount()
        {
            return m_dataAccess.Count();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Dns> Get(int start, int length)
        {
            return m_dataAccess.FetchRange(start, length); 
        }

        [HttpGet("item/{flow-id}/{dns-id}")]
        public Dns Get(string flowId, string dnsId)
        {
            var uuid = Guid.Parse(flowId);
            return m_dataAccess.FetchItem(uuid, dnsId);
        }
                
        // PUT: api/hosts/5
        [HttpPut("item/{address}")]
        public void Put(string address, [FromBody]Dns value)
        {
        }        
    }
}
