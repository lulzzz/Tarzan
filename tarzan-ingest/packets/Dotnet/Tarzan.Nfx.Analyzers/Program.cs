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
using Tarzan.Nfx.Analyzers.Commands;
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
            commandLineApplication.HelpOption("-?|-Help");

            var clusterOption = commandLineApplication.Option("-Cluster <ConnectionString>",
                "Connection string must specify address and the Discovery Spi port of at least one node of a cluster.",
                CommandOptionType.MultipleValue);

            var traceOption = commandLineApplication.Option("-Trace", 
                "If set, it prints various trace information during execution.", 
                CommandOptionType.NoValue);

            commandLineApplication.Command(TrackFlowsCommand.Name, configuration: new TrackFlowsCommand(clusterOption).Configuration);
            commandLineApplication.Command(DetectServicesCommand.Name, configuration: new DetectServicesCommand(clusterOption).Configuration);
            commandLineApplication.Command(ExtractDnsCommand.Name, configuration: new ExtractDnsCommand(clusterOption).Configuration);

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
        }
    }
}
