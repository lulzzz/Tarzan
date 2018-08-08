using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cassandra;
using System.Net;
using Tarzan.Nfx.Model;
using Tarzan.UI.Server.DataAccess;

namespace Tarzan.UI.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/Flows")]
    public class FlowsController : Controller
    {
        IFlowsDataAccess m_dataAccess;

        public FlowsController(IFlowsDataAccess dataAccess)
        {
            m_dataAccess = dataAccess;
        }

        /// <summary>
        /// Gets all flow records.
        /// </summary>
        /// <returns>A collection of all available flow records.</returns>
        [HttpGet("count")]
        public int FetchRecordCount()
        {
            return m_dataAccess.RecordCount();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Flow> FetchRange(int start, int length)
        {
            return m_dataAccess.GetFlowRecords(start, length);
        }
        /// <summary>
        /// Gets the flow record of the specified id.
        /// </summary>
        /// <param name="id">Flow record identifier.</param>
        /// <returns>A flow record of the specified id.</returns>
        [HttpGet("item/{id}")]
        public Flow FetchRecordById(string id)
        {
            var uuid = Guid.Parse(id);
            return m_dataAccess.GetFlowRecord(uuid);
        }
    }
}
