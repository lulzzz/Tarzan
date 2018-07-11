using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Gets some values.
    /// </summary>
    /// <returns>A collection of values</returns>
    [HttpGet]
    public IEnumerable<Capture> Get()
    {
      return new Capture[] { 
        new Capture() {
          Id = 1,
          Name = "testbed-11jun.pcap",
          Type = "pcap",
          Size = 17306938543,
          CreatedOn = DateTime.Parse("2016-01-21T18:57:51"),
          UploadOn = DateTime.Now,
          Author = "Alice Smith",
          Notes = "",
          Tags = new string [] {}
        } };
    }
  }
}
