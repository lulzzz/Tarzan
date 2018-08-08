using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model;
using IFlowsDataAccess = Tarzan.Nfx.Dashboard.DataAccess.ITableDataAccess<Tarzan.Nfx.Model.Flow, System.Guid>;
namespace Tarzan.Nfx.Dashboard.Controllers
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
            return m_dataAccess.Count();
        }

        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Flow> FetchRange(int start, int length)
        {
            return m_dataAccess.FetchRange(start, length);
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
            return m_dataAccess.FetchItem(uuid);
        }
    }
}
