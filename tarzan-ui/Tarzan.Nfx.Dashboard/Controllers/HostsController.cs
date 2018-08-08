using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Tarzan.Nfx.Dashboard.DataAccess;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/hosts")]
    public class HostsController : Controller
    {
        ITableDataAccess<Host,string> m_dataAccess;
        public HostsController(ITableDataAccess<Host, string> dataAccess)
        {
            m_dataAccess = dataAccess;
        }

        [HttpGet("count")]
        public int GetCount()
        {
            return m_dataAccess.Count();
        }

        // GET: api/hosts
        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Host> Get(int start, int length)
        {
            return m_dataAccess.FetchRange(start, length); 
        }

        // GET: api/hosts/5
        [HttpGet("item/{address}")]
        public Host Get(string address)
        {
            return m_dataAccess.FetchItem(address);
        }
                
        // PUT: api/hosts/5
        [HttpPut("item/{address}")]
        public void Put(string address, [FromBody]Host value)
        {
        }        
    }
}
