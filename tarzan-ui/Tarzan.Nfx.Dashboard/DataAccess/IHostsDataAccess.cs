using System.Collections.Generic;
using Tarzan.Nfx.Model;

namespace Tarzan.UI.Server.DataAccess
{
    public interface IHostsDataAccess
    {
        int Count();
        IEnumerable<Host> Fetch(int start, int count);
        Host FetchByAddress(string address);
    }
}