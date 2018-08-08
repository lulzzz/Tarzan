using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model;
using ICapturesDataAccess = Tarzan.Nfx.Dashboard.DataAccess.ITableDataAccess<Tarzan.Nfx.Model.Capture, System.Guid>;
namespace Tarzan.Nfx.Dashboard.Controllers
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
        public IEnumerable<Capture> Get(int start, int length)
        {
            return m_dataAccess.FetchRange(start, length);
        }
        [HttpGet("item/{id}")]
        public Capture Get(string id)
        {
            var uuid = Guid.Parse(id);
            return m_dataAccess.FetchItem(uuid);
        }
        /// <summary>
        /// Gets all flow records.
        /// </summary>
        /// <returns>A collection of all available flow records.</returns>
        [HttpGet("count")]
        public int FetchCount()
        {
            return m_dataAccess.Count();
        }
    }
}
