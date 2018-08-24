using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Lifecycle;
using Microsoft.Extensions.CommandLineUtils;
using Netdx.ConversationTracker;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tarzan.Nfx.Ingest
{
    class StartIgniteServer
    {
        public static string Name => "start-ignite";
        public static Action<CommandLineApplication> Configuration =>
            (CommandLineApplication target) =>
            {
                target.Description = "Start ignite server and waits for jobs from ignite clients.";
                target.OnExecute(() => RunIgniteServer());                
            };
        class ServerLifecycleHandler : ILifecycleHandler
        {
            public void OnLifecycleEvent(LifecycleEventType evt)
            {
                if (evt == LifecycleEventType.AfterNodeStart)
                    Console.WriteLine("Server Node started.");
                else if (evt == LifecycleEventType.AfterNodeStop)
                    Console.WriteLine("Server Node stopped.");
            }

            public bool Started { get; private set; }
        }

        private static int RunIgniteServer()
        {          
            using (var ignite = Ignition.Start(GlobalIgniteConfiguration.Default))
            {
                Console.WriteLine("Ignite server is running, press CTRL+C (or X) to terminate.");

                while (true)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.X)
                        break;
                }
            }
            return 0;
        }
    }
}
