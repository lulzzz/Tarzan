using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;
namespace Tarzan.Nfx.Dashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/Flows")]
    public class FlowsController : Controller
    {
        IAffDataset m_dataset;

        public FlowsController(IAffDataset dataset)
        {
            m_dataset = dataset;
        }

        /// <summary>
        /// Gets all flow records.
        /// </summary>
        /// <returns>A collection of all available flow records.</returns>
        [HttpGet("count")]
        public int FetchRecordCount()
        {
            return (int)m_dataset.FlowTable.Count().Execute();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<FlowData> FetchRange(int start, int length)
        {
            return m_dataset.FlowTable.Execute().Skip(start).Take(length);
        }
        /// <summary>
        /// Gets the flow record of the specified id.
        /// </summary>
        /// <param name="id">Flow record identifier.</param>
        /// <returns>A flow record of the specified id.</returns>
        [HttpGet("item/{uid}")]
        public FlowData FetchRecordById(string uid)
        {
            return m_dataset.FlowTable.Where(x => x.FlowUid == uid).FirstOrDefault().Execute();
        }
    }
}
