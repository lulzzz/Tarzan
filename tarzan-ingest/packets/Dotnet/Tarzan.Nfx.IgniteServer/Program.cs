using Microsoft.Extensions.CommandLineUtils;
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
            var commandLineApplication = new CommandLineApplication(true);

            var configFileArgument = commandLineApplication.Option("-f|--config", "XML configuration file. If not file is specified then default configuration is used.", CommandOptionType.SingleValue);
            var offheapArgument = commandLineApplication.Option("-h|--offheap", "Size of off-heap memory given in megabytes.", CommandOptionType.SingleValue);
            var onheapArgument = commandLineApplication.Option("-g|--onheap", "Size of on-heap memory given in megabytes.", CommandOptionType.SingleValue);
            var leaderNodeArgument = commandLineApplication.Option("-l|--leader", "Set this node as the leader of the cluster.", CommandOptionType.NoValue);
            var serverPortArgument = commandLineApplication.Option("-p|--port", "Specifies port for Discovery Spi.", CommandOptionType.SingleValue);
            var clusterEnpointArgument = commandLineApplication.Option("-c|--cluster", "Specifies IP address and port of a cluster node. Multiple nodes can be specified.", CommandOptionType.MultipleValue);
            var consistentIdArgument = commandLineApplication.Option("-i|--consistentId", "Specifies as a consistent id of the node. This value is used in topology.", CommandOptionType.SingleValue);

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
    }
}
