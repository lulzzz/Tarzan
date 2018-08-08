using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tarzan.UI.Server.DataAccess;
using Tarzan.Nfx.Model;

namespace dashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/hosts")]
    public class HostsController : Controller
    {
        IHostsDataAccess m_dataAccess;
        public HostsController(IHostsDataAccess dataAccess)
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
            return m_dataAccess.Fetch(start, length); 
        }

        // GET: api/hosts/5
        [HttpGet("item/{address}")]
        public Host Get(string address)
        {
            return m_dataAccess.FetchByAddress(address);
        }
                
        // PUT: api/hosts/5
        [HttpPut("item/{address}")]
        public void Put(string address, [FromBody]Host value)
        {
        }        
    }
}
