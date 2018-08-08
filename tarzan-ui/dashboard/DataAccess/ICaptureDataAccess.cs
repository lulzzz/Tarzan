using System;
using System.Collections.Generic;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess
{
    public interface ICaptureDataAccess
    {
        IEnumerable<Capture> GetCaptures(int start = 0, int length = int.MaxValue);
        Capture GetCapture(Guid id);
        int CaptureCount();
    }
}