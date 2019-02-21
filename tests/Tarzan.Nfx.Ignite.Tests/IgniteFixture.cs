using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tarzan.Nfx.IgniteServer;

namespace Tarzan.Nfx.Ignite.Tests
{
    /// <summary>
    /// Represents a test fixture that creates and initializes four Ignite server nodes.
    /// </summary>
    public class IgniteFixture : IDisposable
    {
        private IgniteServerRunner[] _server;
        private Task[] _serverTask;

        public IgniteFixture()
        {
            int numServers = 4;
            _server = new IgniteServerRunner[numServers];
            _serverTask = new Task[numServers];
            for (int i = 0; i < numServers; i++)
            {
                _server[i] = new IgniteServerRunner();
                _serverTask[i] = Server.Run();
            }

        }

        public IgniteServerRunner Server => _server[0];

        public void Dispose()
        {
            foreach (var server in _server)
            {
                server.Stop();
            }
            Task.WaitAll(_serverTask);
        }
    }
}
