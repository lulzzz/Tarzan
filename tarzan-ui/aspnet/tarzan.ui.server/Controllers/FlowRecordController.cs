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


        [HttpGet]
        [Route("api/flows/index")]
        public IActionResult Index()
        {                        
            return Json(m_dataAccess.GetAllFlowRecords());
        }
        [HttpGet]
        [Route("api/flows/item")]  
        public IActionResult FetchRecordById(int id)
        {
            return Json(m_dataAccess.GetFlowRecord(id));
        }
    }
}
