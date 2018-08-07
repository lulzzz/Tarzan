using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cassandra;
using System.Net;
using Tarzan.UI.Server.Models;
using Tarzan.UI.Server.DataAccess;

namespace Tarzan.UI.Server.Controllers
{
    [Route("api/[controller]")]
    public class FlowsController : Controller
    {
        IFlowRecordDataAccess m_dataAccess;

        public FlowsController(IFlowRecordDataAccess dataAccess)
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
        public IEnumerable<FlowRecord> FetchRange(int start, int length)
        {
            return m_dataAccess.GetFlowRecords(start, length);
        }
        /// <summary>
        /// Gets the flow record of the specified id.
        /// </summary>
        /// <param name="id">Flow record identifier.</param>
        /// <returns>A flow record of the specified id.</returns>
        [HttpGet("item/{id}")]
        public FlowRecord FetchRecordById(string id)
        {
            var uuid = Guid.Parse(id);
            return m_dataAccess.GetFlowRecord(uuid);
        }
    }
}
