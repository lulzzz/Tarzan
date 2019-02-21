using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;
namespace Tarzan.Nfx.Dashboard.Controllers
{

    [Produces("application/json")]
    [Route("api/captures")]
    public class CapturesController : Controller
    {
        IAffDataset m_dataset;

        public CapturesController(IAffDataset dataset)
        {
            m_dataset = dataset;
        }
        /// <summary>
        /// Gets some values.
        /// </summary>
        /// <returns>A collection of values</returns>
        [HttpGet("range/{start}/count/{length}")]
        public IEnumerable<Capture> Get(int start, int length)
        {
            return m_dataset.CaptureTable.Execute().Skip(start).Take(length);
        }
        [HttpGet("item/{id}")]
        public Capture Get(string uid)
        {
            return m_dataset.CaptureTable.Where(x => x.Uid == uid).FirstOrDefault().Execute();
        }
        /// <summary>
        /// Gets all flow records.
        /// </summary>
        /// <returns>A collection of all available flow records.</returns>
        [HttpGet("count")]
        public int FetchCount()
        {
            return (int)m_dataset.CaptureTable.Count().Execute();
        }
    }
}
