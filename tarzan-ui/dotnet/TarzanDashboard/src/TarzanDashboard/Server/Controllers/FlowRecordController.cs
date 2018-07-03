using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cassandra;
using System.Net;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.Controllers
{
    
    [Produces("application/json")]
    public class FlowRecordController : Controller
    {
        FlowRecordDataAccess m_dataAccess = new FlowRecordDataAccess(new IPEndPoint(IPAddress.Loopback, 9042) , "flowstat");


        /// <summary>
        /// Gets all flow records.
        /// </summary>
        /// <returns>A collection of all available flow records.</returns>
        [HttpGet]
        [Route("api/flows/index")]
        public IEnumerable<FlowRecord> Index()
        {                        
            return m_dataAccess.GetAllFlowRecords();
        }
        /// <summary>
        /// Gets the flow record of the specified id.
        /// </summary>
        /// <param name="id">Flow record identifier.</param>
        /// <returns>A flow record of the specified id.</returns>
        [HttpGet]
        [Route("api/flows/item")]  
        public FlowRecord FetchRecordById(int id)
        {
            return m_dataAccess.GetFlowRecord(id);
        }
    }
}
