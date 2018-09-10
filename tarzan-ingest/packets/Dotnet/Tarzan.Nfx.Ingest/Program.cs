using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpPcap;
using System;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    /// <summary>
    /// Computes the flow statististics for the given input pcap file.
    /// It also creates a hierarchy of bloom filters to improve the access to individual packets. 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();


            var trackFlowsCmd = new TrackFlows(serviceProvider);
            var startIgniteServerCmd = new StartIgniteServer(serviceProvider);
            var commandLineApplication = new CommandLineApplication(true);


            commandLineApplication.Command(trackFlowsCmd.Name, trackFlowsCmd.Configuration);
            commandLineApplication.Command(PrintInterfaces.Name, PrintInterfaces.Configuration);           
            commandLineApplication.Command(startIgniteServerCmd.Name, startIgniteServerCmd.Configuration);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.Name = typeof(Program).Assembly.GetName().Name;
            commandLineApplication.FullName = $"Tarzan.Nfx Packet Traces Ingestor ({typeof(Program).Assembly.GetName().Version})";

            commandLineApplication.OnExecute(() =>
            {
                commandLineApplication.Error.WriteLine();
                commandLineApplication.ShowHelp();
                return -1;
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
            catch (PcapException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            serviceCollection.AddSingleton(loggerFactory);

            var igniteConfiguration = GlobalIgniteConfiguration.GetDefaultIgniteConfiguration();
            serviceCollection.AddSingleton(igniteConfiguration);
        }
    }
}
