using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tarzan.Nfx.IgniteServer;

namespace Tarzan.Nfx.Ignite.Tests
{
    public class IgniteFixture : IDisposable
    {
        private IgniteServerRunner _server;
        private Task _serverTask;

        public IgniteFixture()
        {
            _server = new IgniteServerRunner();
            _serverTask = Server.Run();
        }

        public IgniteServerRunner Server => _server;

        public void Dispose()
        {
            _server.Stop();
            _serverTask.Wait();
        }
    }
}
