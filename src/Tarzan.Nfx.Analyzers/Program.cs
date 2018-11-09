using Microsoft.Extensions.CommandLineUtils;
using NLog;
using NLog.Config;
using NLog.Targets;
using Tarzan.Nfx.Analyzers.Commands;

namespace Tarzan.Nfx.Analyzers
{
    /// <summary>
    /// Implements UI for FlowTracker. 
    /// To execute FlowTracker programatically from the code use: <code>compute.Broadcast(new FlowAnalyzer() { CacheName = cacheName });</code>
    /// </summary>
    partial class Program
    { 
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            SetupLogging(true);            
            logger.Debug("Application started.");

            var commandLineApplication = new CommandLineApplication();
            commandLineApplication.Name = "Tarzan.Nfx.Analyzers";
            commandLineApplication.HelpOption("-?|-Help");

            var clusterOption = commandLineApplication.Option("-Cluster <ConnectionString>",
                "Connection string must specify address and the Discovery Spi port of at least one node of a cluster.",
                CommandOptionType.MultipleValue);

            var LogLevel = commandLineApplication.Option("-LogLevel", 
                "If set, it prints various trace information during execution.", 
                CommandOptionType.SingleValue);

            commandLineApplication.Command(TrackFlowsCommand.Name, configuration: new TrackFlowsCommand(clusterOption).Configuration);
            commandLineApplication.Command(DetectServicesCommand.Name, configuration: new DetectServicesCommand(clusterOption).Configuration);
            commandLineApplication.Command(ExtractDnsCommand.Name, configuration: new ExtractDnsCommand(clusterOption).Configuration);
            commandLineApplication.Command(ExtractDnsCommand.Name, configuration: new ExtractDnsCommand(clusterOption).Configuration);
            commandLineApplication.Command(ExtractTcpSTMCommand.Name, configuration: new ExtractTcpSTMCommand(clusterOption).Configuration);

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
            logger.Debug("Application ended.");
        }
        static void SetupLogging(bool logToConsole)
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();
            
            // Step 2. Create targets
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = @"[${date:format=HH\:mm\:ss}] ${level}: ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);

            var fileTarget = new FileTarget("errorfile")
            {
                FileName = "${basedir}/Tarzan.Nfx.Analyzers.err",
                Layout = "[${longdate}] ${level}: ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);

            config.AddRuleForOneLevel(LogLevel.Error, fileTarget);
            // Step 3. Define rules     
            if (logToConsole)       
                config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget); 

            // Step 4. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}
