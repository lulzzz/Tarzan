using Microsoft.Extensions.CommandLineUtils;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;

namespace Tarzan.Nfx.IgniteServer
{
    class Program
    {
        const int DefaultOffHeapMemory = 4096;
        const int DefaultOnHeapMemory = 4096;
        readonly string [] DefaultClusterEnpoints = new [] { "127.0.0.1:47500" };

        enum EventIds : int { EVT_METRICS, EVT_IGNITE_STATUS };

        static void Main(string[] args)
        {
            SetupLogging();
            var commandLineApplication = new CommandLineApplication(true);
            commandLineApplication.Name = "Tarzan.Nfx.IgniteServer";
            commandLineApplication.HelpOption("-?|-Help");
            var configFileArgument = commandLineApplication.Option("-ConfigFile", "XML configuration file. If not file is specified then default configuration is used.", CommandOptionType.SingleValue);
            var offheapArgument = commandLineApplication.Option("-OffHeap", "Size of off-heap memory given in megabytes.", CommandOptionType.SingleValue);
            var onheapArgument = commandLineApplication.Option("-OnHeap", "Size of on-heap memory given in megabytes.", CommandOptionType.SingleValue);
            var leaderNodeArgument = commandLineApplication.Option("-SetLeader", "Set this node as the leader of the cluster.", CommandOptionType.NoValue);
            var serverPortArgument = commandLineApplication.Option("-SpiPort", "Specifies port for Discovery Spi.", CommandOptionType.SingleValue);
            var clusterEnpointArgument = commandLineApplication.Option("-Cluster", "Specifies IP address and port of a cluster node. Multiple nodes can be specified.", CommandOptionType.MultipleValue);
            var consistentIdArgument = commandLineApplication.Option("-ConsistentId", "Specifies as a consistent id of the node. This value is used in topology.", CommandOptionType.SingleValue);
            var persistenceEnabled = commandLineApplication.Option("-PersistenceEnabled", "If set, it enables persistence mode.", CommandOptionType.NoValue);
            commandLineApplication.OnExecute(async () =>
            {
                var configFile = configFileArgument.HasValue() ? configFileArgument.Value() : null;

                using (var server = new IgniteServerRunner(configFile))
                {
                    if (offheapArgument.HasValue()) server.SetOffHeapMemoryLimit(Int32.Parse(offheapArgument.Value()));
                    if (onheapArgument.HasValue()) server.SetOnHeapMemoryLimit(Int32.Parse(onheapArgument.Value()));
                    if (serverPortArgument.HasValue()) server.SetServerPort(Int32.Parse(serverPortArgument.Value()));
                    if (clusterEnpointArgument.HasValue()) server.SetClusterEnpoints(clusterEnpointArgument.Values);
                    if (consistentIdArgument.HasValue()) server.SetConsistentId(consistentIdArgument.Value());
                    if (persistenceEnabled.HasValue()) server.SetPersistence(true);
                    await server.Run();
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

        static void SetupLogging()
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
                FileName = "${basedir}/Tarzan.Nfx.IgniteServer.log",
                Layout = "[${longdate}] ${level}: ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);


            // Step 3. Define rules            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget); // all to console

            // Step 4. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}
