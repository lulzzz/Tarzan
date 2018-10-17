using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Deployment;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tarzan.Nfx.IgniteServer
{
    class Program
    {
        const int DefaultOffHeapMemory = 4096;
        const int DefaultOnHeapMemory = 4096;

        enum EventIds : int { EVT_METRICS, EVT_IGNITE_STATUS };

        private ConsoleLogger m_logger = new ConsoleLogger("Tarzan.IgniteServer", (s, ll) => true, true);

        void PrintStats(IIgnite ignite)
        {
            if (ignite.IsActive())
            {
                var regionsMetrics = ignite.GetDataRegionMetrics();

                // Print out some of the metrics.
                foreach (var metrics in regionsMetrics)
                {
                    m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Metrics", (int)EventIds.EVT_METRICS, $"Memory Region Name: {metrics.Name}, " +
                        $"Allocation Rate: {metrics.AllocationRate} pps, " +
                        $"Fill Factor: {metrics.PageFillFactor}, " +
                        $"Allocated Size: {metrics.TotalAllocatedSize} bytes, " + 
                        $"Physical Memory Size: { metrics.PhysicalMemorySize} bytes.", null);
                }
            }
        }



        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);

            var offheapArgument = commandLineApplication.Option("-h|--offheap", "Size of off-heap memory given in megabytes.", CommandOptionType.SingleValue);
            var onheapArgument = commandLineApplication.Option("-g|--onheap", "Size of on-heap memory given in megabytes.", CommandOptionType.SingleValue);
            var leaderNode = commandLineApplication.Option("-l|--leader", "Set this node as the leader of the cluster.", CommandOptionType.NoValue);
            
            commandLineApplication.OnExecute(async () =>
            {
                var maxOffHeapMemory = offheapArgument.HasValue() ? Int32.Parse(offheapArgument.Value()) : DefaultOffHeapMemory;
                var maxOnHeapMemory = onheapArgument.HasValue() ? Int32.Parse(onheapArgument.Value()) : DefaultOnHeapMemory;
                await new Program().RunServer(maxOffHeapMemory, maxOnHeapMemory, leaderNode.HasValue());
                return 0;
            });


            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
                commandLineApplication.ShowHelp();
            }
            catch (ArgumentException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
                commandLineApplication.ShowHelp();
            }
        }

        private async Task RunServer(long maxOffHeap, long maxOnHeap, bool leader = false)
        {
            var cfg = new IgniteConfiguration
            {
                JvmOptions = new[] { //"-Xms256m",
                                     $"-Xmx{maxOnHeap}m",
                                     "-XX:+AlwaysPreTouch",
                                     "-XX:+UseG1GC",
                                     "-XX:+ScavengeBeforeFullGC",
                                     "-XX:+DisableExplicitGC",
                                     $"-XX:MaxDirectMemorySize={maxOffHeap}m"
                },
                DataStorageConfiguration = new DataStorageConfiguration
                {
                    DefaultDataRegionConfiguration = new DataRegionConfiguration
                    {
                        Name = "default",
                        PersistenceEnabled = true,
                        MaxSize = maxOffHeap * 1024 * 1024,
                    },
                }, 
                PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,
                DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        Endpoints = new[] { "127.0.0.1:47500" }
                    },
                }
            };
            using (var ignite = Ignition.Start(cfg))
            {
                var cts = new CancellationTokenSource();
                ignite.Stopped += (s, e) => cts.Cancel();
                var localSpidPort = (ignite.GetConfiguration().DiscoverySpi as TcpDiscoverySpi).LocalPort;
                m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Status", (int)EventIds.EVT_IGNITE_STATUS, $"Ignite server is running (Local SpiDiscovery Port={localSpidPort}), press CTRL+C to terminate.", null);
                ignite.GetCluster().SetActive(true);
                try
                {
                    while (!cts.IsCancellationRequested)
                    {
                        //PrintStats(ignite);
                        await Task.Delay(5000, cts.Token);
                    }
                }
                catch (Exception e) { }
                m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Status", (int)EventIds.EVT_IGNITE_STATUS, "Ignite Server exited gracefully.", null);
            }
        }
    }
}
