using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Resource;
using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
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
        
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication();
            commandLineApplication.Name = "Tarzan.Nfx.Analyzers";
            commandLineApplication.HelpOption("-?|-h|--help");

            var clusterOption = commandLineApplication.Option("-c|--cluster <ConnectionString>",
                "Connection string must specify address and the Discovery Spi port of at least one node of a cluster.",
                CommandOptionType.MultipleValue);

            var traceOption = commandLineApplication.Option("-t|--trace", 
                "If set, it prints various trace information during execution.", 
                CommandOptionType.NoValue);

            commandLineApplication.Command(TrackFlowsCommand.Name, configuration: new TrackFlowsCommand(clusterOption).Configuration);

            commandLineApplication.OnExecute(() => {
                commandLineApplication.Error.WriteLine("Error: Command not specified!");
                commandLineApplication.ShowHelp();
                return 0;
            });

            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException e)
            {
                commandLineApplication.Error.WriteLine($"Error: {e.Message}");
                commandLineApplication.ShowHelp();
            }
            /*
            commandLineApplication.OnExecute(async () =>
            {

                var endpoints = clusterArgument.HasValue() ? clusterArgument.Values.ToArray() : new[] { DEFAULT_EP };
                

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
                    
                        if (traceArgument.HasValue())
                        {
                            var frameCache = ignite.GetCache<FrameKey, Frame>(cacheName);
                            var flowCache = ignite.GetOrCreateCache<FlowKey, PacketFlow>(PacketFlow.CACHE_NAME);
                            Console.WriteLine($"Done. Frames={frameCache.GetSize()}, Flows={flowCache.GetSize()}.");
                            Console.WriteLine($"Top 10 Flows:");
                            foreach (var flow in flowCache.OrderByDescending(x=>x.Value.Octets).Take(10))
                            {
                                var scanQuery = new ScanQuery<FrameKey, Frame>(new CacheEntryFrameFilter(flow.Key));
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
            */
        }
    }
}
