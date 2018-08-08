using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.UI.Server.DataAccess
{
    public interface ICapturesDataAccess
    {
        IEnumerable<Capture> GetCaptures(int start = 0, int length = int.MaxValue);
        Capture GetCapture(Guid id);
        int CaptureCount();
    }
}