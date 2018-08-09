using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Tarzan.Nfx.Dashboard.DataAccess;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/services")]
    public class ServicesController : Controller
    {
        ITableDataAccess<Service, string> m_dataAccess;
        public ServicesController(ITableDataAccess<Service, string> dataAccess)
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
        public IEnumerable<Service> Get(int start, int length)
        {
            return m_dataAccess.FetchRange(start, length); 
        }

        // GET: api/hosts/5
        [HttpGet("item/{address}")]
        public Service Get(string address)
        {
            return m_dataAccess.FetchItem(address);
        }
                
        // PUT: api/hosts/5
        [HttpPut("item/{address}")]
        public void Put(string address, [FromBody]Service value)
        {
        }        
    }
}
