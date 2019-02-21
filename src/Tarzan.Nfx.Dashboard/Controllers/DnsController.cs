using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard
{
    [Produces("application/json")]
    [Route("api/dns")]
    public class DnsController : Controller
    {
        IAffDataset m_dataset;
        public DnsController(IAffDataset dataset)
        {
            m_dataset = dataset;
        }

        [HttpGet("count")]
        public int GetCount()
        {
            return (int)m_dataset.DnsTable.Count().Execute();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<DnsObject> Get(int start, int length)
        {
            return m_dataset.DnsTable.Execute().Skip(start).Take(length);
        }

        [HttpGet("item/{flowUid}/{transactionId}")]
        public DnsObject Get(string flowUid, string transactionId)
        {
            return m_dataset.DnsTable.Where(x => x.FlowUid == flowUid && x.TransactionId == transactionId).FirstOrDefault().Execute();
        }
    }
}
