using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Tarzan.UI.Server.DataAccess;
using Tarzan.UI.Server.Models;

namespace TarzanDashboard.Controllers
{
    /// <summary>
    /// Values Controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    public class CapturesController : Controller
    {
        ICaptureDataAccess m_dataAccess;

        public CapturesController(ICaptureDataAccess dataAccess)
        {
            m_dataAccess = dataAccess;
        }
        /// <summary>
        /// Gets some values.
        /// </summary>
        /// <returns>A collection of values</returns>
        [HttpGet()]
        public IEnumerable<Capture> Get()
        {
            return m_dataAccess.GetAllCaptures();
        }
    }
}
