using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Deployment;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Threading;

namespace Tarzan.Nfx.IgniteServer
{
    class Program
    {
        const int DefaultOffHeapMemory = 1024;
        const int DefaultThreads = 8;
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);

            var offheapArgument = commandLineApplication.Option("-m|--offheap", "Size of offheap memory given in megabytes.", CommandOptionType.SingleValue);
            var threadsArgument = commandLineApplication.Option("-t|--threads", "Size of a thread pool used by the server.", CommandOptionType.SingleValue);

            
            commandLineApplication.OnExecute(() =>
            {
                var maxThreads = threadsArgument.HasValue() ? Math.Min(Environment.ProcessorCount, Int32.TryParse(threadsArgument.Value(), out var requiredCores) ? requiredCores : DefaultThreads) : DefaultThreads;
                var maxOffHeapMemory = offheapArgument.HasValue() ? Int32.Parse(offheapArgument.Value()) : DefaultOffHeapMemory;

                Console.WriteLine($"Tarzan.Nfx.IgniteServer [maxThreads={maxThreads}, maxOffHeap={maxOffHeapMemory}MB]");
                var cfg = new IgniteConfiguration
                {
                    PublicThreadPoolSize = maxThreads,
                    DataStorageConfiguration = new DataStorageConfiguration
                    {
                        DefaultDataRegionConfiguration = new DataRegionConfiguration
                        {
                            Name = "defaultRegion",
                            PersistenceEnabled = true,
                        },
                    },
                    PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,
                    DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                    {
                        IpFinder = new TcpDiscoveryStaticIpFinder
                        {
                            Endpoints = new[] { "127.0.0.1:47500..47501" }
                        },
                    }


                };
                using (var ignite = Ignition.Start(cfg))
                {
                    var mem = ignite.GetDataRegionMetrics();
                    var cts = new CancellationTokenSource();
                    ignite.Stopped += (s, e) => cts.Cancel();
                    var localSpidPort = (ignite.GetConfiguration().DiscoverySpi as TcpDiscoverySpi).LocalPort;
                    Console.WriteLine($"Ignite server is running (Local SpiDiscovery Port={localSpidPort}), press CTRL+C to terminate.");
                    ignite.GetCluster().SetActive(true);
                    cts.Token.WaitHandle.WaitOne();
                    Console.WriteLine("Ignite Server Gracefully Shutdowned.");
                }
                return 0;
            });


            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
            }
            catch (ArgumentException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
            }
        }
    }
}
