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
    public class FlowRecordController : Controller
    {
        IFlowRecordDataAccess m_dataAccess;

        public FlowRecordController(IFlowRecordDataAccess dataAccess)
        {
            m_dataAccess = dataAccess;
        }

        /// <summary>
        /// Gets all flow records.
        /// </summary>
        /// <returns>A collection of all available flow records.</returns>
        [HttpGet()]
        public IEnumerable<FlowRecord> Index()
        {                        
            return m_dataAccess.GetAllFlowRecords();
        }
        /// <summary>
        /// Gets the flow record of the specified id.
        /// </summary>
        /// <param name="id">Flow record identifier.</param>
        /// <returns>A flow record of the specified id.</returns>
        [HttpGet("{id}")]
        public FlowRecord FetchRecordById(int id)
        {
            return m_dataAccess.GetFlowRecord(id);
        }
    }
}
