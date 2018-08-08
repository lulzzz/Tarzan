using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Tarzan.UI.Server.DataAccess;
using Tarzan.Nfx.Model;

namespace TarzanDashboard.Controllers
{

    [Produces("application/json")]
    [Route("api/captures")]
    public class CapturesController : Controller
    {
        ICapturesDataAccess m_dataAccess;

        public CapturesController(ICapturesDataAccess dataAccess)
        {
            m_dataAccess = dataAccess;
        }
        /// <summary>
        /// Gets some values.
        /// </summary>
        /// <returns>A collection of values</returns>
        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Capture> FetchRange(int start, int length)
        {
            return m_dataAccess.GetCaptures(start, length);
        }
        /// <summary>
        /// Gets all flow records.
        /// </summary>
        /// <returns>A collection of all available flow records.</returns>
        [HttpGet("count")]
        public int FetchCount()
        {
            return m_dataAccess.CaptureCount();
        }
    }
}
