using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Commands
{
    class ExtractDnsCommand : AbstractCommand
    {
        private static NLog.Logger m_logger = NLog.LogManager.GetCurrentClassLogger();

        public static string Name { get; private set; } = "Extract-Dns";

        public ExtractDnsCommand(CommandOption clusterOption) : base(clusterOption)
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

        private int ExecuteCommand(IIgnite ignite, List<string> flowCacheNames, List<string> packetCacheNames, string dnsOutCacheName)
        {
            var compute = ignite.GetCompute();
            m_logger.Info($"Compute cluster nodes: [{String.Join(",", compute.ClusterGroup.GetNodes().Select(n => n.Id.ToString()))}].");

            foreach (var flowCacheName in flowCacheNames)
            {
                var dnsAnalyzer = new DnsAnalyzer(flowCacheName, packetCacheNames, dnsOutCacheName);
                m_logger.Info($"Source flow cache: {flowCacheName}.");
                m_logger.Info($"Broadcasting compute action: {typeof(DnsAnalyzer).AssemblyQualifiedName}...");
                compute.Broadcast(dnsAnalyzer);
                m_logger.Info("Remote computation done.");
                var dnsCache = ignite.GetOrCreateCache<string, DnsObject>(dnsOutCacheName);
                m_logger.Info($"Found {dnsCache.GetSize()} dns objects.");
            }
            return 0;
        }
    }
}
