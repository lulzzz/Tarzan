using Apache.Ignite.Core;
using Apache.Ignite.Core.Deployment;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tarzan.Nfx.IgniteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Tarzan.Nfx.IgniteServer");
            var config = new IgniteConfiguration
            {
                PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain
            };
            using (var ignite = Ignition.Start(config))
            {
                var cts = new CancellationTokenSource();
                ignite.Stopped += (s, e) => cts.Cancel();
                Console.WriteLine("Ignite server is running, press CTRL+C to terminate.");
                cts.Token.WaitHandle.WaitOne();
                Console.WriteLine("Ignite Server Gracefully Shutdowned.");
            }
        }
    }
}
