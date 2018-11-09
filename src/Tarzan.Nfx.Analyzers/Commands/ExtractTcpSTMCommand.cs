using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Analyzers.Tcp;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Commands
{
    class ExtractTcpSTMCommand : AbstractCommand
    {
        private static NLog.Logger m_logger = NLog.LogManager.GetCurrentClassLogger();

        public static string Name { get; private set; } = "Extract-Tcp";

        public ExtractTcpSTMCommand(CommandOption clusterOption) : base(clusterOption)
        {

        }

        public override void Configuration(CommandLineApplication command)
        {
            command.Description = "Identifies all flows in the given captures.";
            command.HelpOption("-?|-Help");

            var flowsOption = command.Option("-FlowCache <CacheName>",
                        "A name of cache that contains flow objects. Multiple values can be specified.",
                        CommandOptionType.MultipleValue);

            var packetsOption = command.Option("-PacketCache <CacheName>",
                        "A name of the cache that contains packets.",
                        CommandOptionType.SingleValue);

            var writeToOption = command.Option("-WriteTo <CacheName>",
            "A name of the cache that contains packets.",
            CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                using (var ignite = CreateIgniteClient())
                {
                    return ExecuteCommand(ignite.Start(), flowsOption.Values, packetsOption.Values, writeToOption.Value());
                }
            });
        }

        private int ExecuteCommand(IIgnite ignite, List<string> flowCacheNames, List<string> packetCacheNames, string tcpOutCacheName)
        {
            var compute = ignite.GetCompute();
            m_logger.Info($"Compute cluster nodes: [{String.Join(",", compute.ClusterGroup.GetNodes().Select(n => n.Id.ToString()))}].");

            foreach (var flowCacheName in flowCacheNames)
            {
                var tcpExtractor = new TcpSpaceTimeExtractor(flowCacheName, packetCacheNames, tcpOutCacheName);
                m_logger.Info($"Source flow cache: {flowCacheName}.");
                m_logger.Info($"Broadcasting compute action: {typeof(TcpSpaceTimeExtractor).AssemblyQualifiedName}...");
                compute.Broadcast(tcpExtractor);
                m_logger.Info("Remote computation done.");
                var tcpCache = ignite.GetOrCreateCache<FlowKey, TcpSpaceTimeModel>(tcpOutCacheName);
                m_logger.Info($"Found {tcpCache.GetSize()} tcp objects.");
            }
            return 0;
        }
    }
}
