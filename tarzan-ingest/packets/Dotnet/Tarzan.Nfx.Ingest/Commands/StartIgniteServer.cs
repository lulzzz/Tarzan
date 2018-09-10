using Apache.Ignite.Core;
using Apache.Ignite.Core.Lifecycle;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Tarzan.Nfx.Ingest
{
    class StartIgniteServer : IApplicationCommand
    {
        private readonly IServiceProvider m_serviceProvider;

        public StartIgniteServer(IServiceProvider serviceProvider)
        {
            m_serviceProvider = serviceProvider;
        }

        public string Name => "start-ignite";

        public void ExecuteCommand(CommandLineApplication target)
        {
            target.Description = "Start ignite server and waits for jobs from ignite clients.";
            target.OnExecute(() => RunIgniteServer());
        }

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

        private int RunIgniteServer()
        {
            var config = m_serviceProvider.GetService<IgniteConfiguration>();

            using (var ignite = Ignition.Start(config))
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
