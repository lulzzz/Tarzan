using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/hosts")]
    public class HostsController : Controller
    {
        IAffDataset m_dataset;
        public HostsController(IAffDataset dataset)
        {
            m_dataset = dataset;
        }

        [HttpGet("count")]
        public int GetCount() => (int)m_dataset.HostTable.Count().Execute();

        // GET: api/hosts
        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Host> Get(int start, int length)
        {
            return m_dataset.HostTable.Execute().Skip(start).Take(length);
        }

        // GET: api/hosts/5
        [HttpGet("item/{address}")]
        public Host Get(string address)
        {
            return m_dataset.HostTable.Where(x => x.Address == address).FirstOrDefault().Execute();
        }     
    }
}
