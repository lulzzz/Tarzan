using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Multicast;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Resource;
using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
using System;
using System.Linq;
using System.Net;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.Analyzers
{
    /// <summary>
    /// Implements UI for FlowTracker. 
    /// To execute FlowTracker programatically from the code use: <code>compute.Broadcast(new FlowAnalyzer() { CacheName = cacheName });</code>
    /// </summary>
    partial class Program
    {
        const string DEFAULT_EP = "127.0.0.1:47500";
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);
            var clusterArgument = commandLineApplication.Option("-c|--cluster", "Enpoint string of any cluster node.", CommandOptionType.MultipleValue);
            var cacheArgument = commandLineApplication.Option("-s|--cache", "Name of the cache with frames to process.", CommandOptionType.MultipleValue);
            var loadArgument = commandLineApplication.Option("-l|--load", "Name of the file to be loaded into the cache.", CommandOptionType.MultipleValue);
            var traceArgument = commandLineApplication.Option("-t|--trace", "If set, it various trace information during execution.", CommandOptionType.NoValue);
            commandLineApplication.OnExecute(async () =>
            {

                var endpoints = clusterArgument.HasValue() ? clusterArgument.Values.ToArray() : new[] { DEFAULT_EP };
                var cfg = new IgniteConfiguration
                {
                    PeerAssemblyLoadingMode = Apache.Ignite.Core.Deployment.PeerAssemblyLoadingMode.CurrentAppDomain,
                    DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                    {
                        IpFinder = new TcpDiscoveryMulticastIpFinder
                        {
                            MulticastGroup = "228.10.10.157",
                            Endpoints = endpoints
                        }
                    },
                };

                if (loadArgument.HasValue())
                {
                    Console.WriteLine("Loading frames...");
                    var pcapLoader = new Tarzan.Nfx.PcapLoader.PcapLoader();
                    foreach (var fileName in loadArgument.Values)
                    {
                        Console.WriteLine($"Loading frames from'{fileName}'...");
                        pcapLoader.SourceFiles.Add(new System.IO.FileInfo(fileName));
                    }
                    await pcapLoader.Invoke();
                    Console.WriteLine("All frames loaded.");
                }

                Ignition.ClientMode = true;
                Console.WriteLine($"Starting cluster client...");
                Console.WriteLine($"DiscoverySpi={String.Join(",", endpoints)}");
                using (var ignite = Ignition.Start(cfg))
                {
                    Console.WriteLine("Client started!");
                    var cluster = ignite.GetCluster();
                    var compute = ignite.GetCompute();

                    
                    foreach (var cacheName in cacheArgument.Values)
                    {
                        Console.WriteLine($"Tracking flows in {cacheName}");
                        
                        Console.WriteLine($"Compute is {String.Join(",",compute.ClusterGroup.GetNodes().Select(n=>n.Id.ToString()))}");

                       

                        var flowAnalyzer = new FlowAnalyzer()
                        {
                            CacheName = cacheName,
                            Progress = new ConsoleProgressReport()
                        };
                        
                        compute.Broadcast(flowAnalyzer);

                        if (traceArgument.HasValue())
                        {
                            var frameCache = ignite.GetCache<FrameKey, Frame>(cacheName);
                            var flowCache = ignite.GetOrCreateCache<FlowKey, PacketFlow>(PacketFlow.CACHE_NAME);
                            Console.WriteLine($"Done. Frames={frameCache.GetSize()}, Flows={flowCache.GetSize()}.");
                            Console.WriteLine($"Top 10 Flows:");
                            foreach (var flow in flowCache.OrderByDescending(x=>x.Value.Octets).Take(10))
                            {
                                var scanQuery = new ScanQuery<FrameKey, Frame>(new CacheEntryFrameFilter(flow.Key));
                                //var scanQuery = new ScanQuery<FrameKey, Frame>(new CacheEntryFrameFilter2());
                                var queryCursor = frameCache.Query(scanQuery);
                                Console.WriteLine($"  Flow   {flow.Key.ToString()}:");
                                var getFrameKey = new FrameKeyProvider();
                                foreach (var cacheEntry in queryCursor)
                                {
                                    var key = getFrameKey.GetKey(cacheEntry.Value);
                                    Console.WriteLine($"    [key={key},ts={cacheEntry.Value.Timestamp}, len={cacheEntry.Value.Data.Length}]");
                                }
                            }
                        }
                    }
                }
                if (traceArgument.HasValue())
                {
                    Console.Write("Press any key to continue...");
                    Console.ReadKey();
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
                commandLineApplication.ShowHelp();
            }
            catch (ArgumentException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
                commandLineApplication.ShowHelp();
            }
        }

        public class ConsoleProgressReport : IProgress<FlowAnalyzer.ProgressRecord>
        {
            public ConsoleProgressReport()
            {
            }
            public void Report(FlowAnalyzer.ProgressRecord value)
            {
                Console.WriteLine($"\rProgress: Frames={value.CompletedFrames}/{value.TotalFrames}, Flows={value.CompletedFlows}/{value.TotalFlows}, Elapsed={value.ElapsedTime.ElapsedMilliseconds}ms.");
            }
        }
    }
}
