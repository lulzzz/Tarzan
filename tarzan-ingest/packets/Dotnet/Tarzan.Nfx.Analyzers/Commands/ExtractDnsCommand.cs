using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Commands
{
    class ExtractDnsCommand : AbstractCommand
    {
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
            Console.WriteLine($"Compute is {String.Join(",", compute.ClusterGroup.GetNodes().Select(n => n.Id.ToString()))}");

            foreach (var flowCacheName in flowCacheNames)
            {
                var dnsAnalyzer = new DnsAnalyzer(flowCacheName, packetCacheNames, dnsOutCacheName);
                Console.Write($"Analyzing {flowCacheName}...");
                compute.Broadcast(dnsAnalyzer);
                Console.WriteLine("Done.");
                var dnsCache = ignite.GetOrCreateCache<string, DnsObject>(dnsOutCacheName);
                Console.WriteLine($"Processed dns={dnsCache.GetSize()}.");
            }
            return 0;
        }
    }
}
