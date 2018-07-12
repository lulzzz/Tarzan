using System.Collections.Generic;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess
{
    public interface ICaptureDataAccess
    {
        IEnumerable<Capture> GetAllCaptures(int limit = int.MaxValue);
        Capture GetCapture(int id);
    }
}