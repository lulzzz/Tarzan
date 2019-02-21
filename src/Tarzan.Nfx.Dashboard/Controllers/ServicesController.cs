using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/services")]
    public class ServicesController : Controller
    {
        IAffDataset m_dataset;
        public ServicesController(IAffDataset dataset)
        {
            m_dataset = dataset;
        }

        [HttpGet("count")]
        public int GetCount()
        {
            return (int)m_dataset.ServiceTable.Count().Execute();
        }

        // GET: api/hosts
        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Service> Get(int start, int length)
        {
            return m_dataset.ServiceTable.Execute().Skip(start).Take(length);
        }

        // GET: api/hosts/5
        [HttpGet("item/{serviceName}")]
        public Service Get(string serviceName)
        {
            return m_dataset.ServiceTable.Where(x => x.ServiceName == serviceName).FirstOrDefault().Execute();
        }                
    }
}
