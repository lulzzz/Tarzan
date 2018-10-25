using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;

namespace Tarzan.Nfx.Analyzers
{
    class TrackFlowsCommand
    {
        private CommandOption m_clusterOption;

        public TrackFlowsCommand(CommandOption clusterOption)
        {
            this.m_clusterOption = clusterOption;
        }

        public static string Name { get; private set; } = "track-flows";

        internal void Configuration(CommandLineApplication command)
        {
            command.Description = "Identifies all flows in the given captures.";
            command.HelpOption("-?|-h|--help");

            var readOption = command.Option("-r|--read <CacheName>",
                        "A name of cache that contains source frames. Multiple values can be specified.",
                        CommandOptionType.MultipleValue);

            var writeOption = command.Option("-w|--write <CacheName>",
                        "A Possibly fresh name of the cache that will be populated with identified flows.",
                        CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                using (var ignite = new IgniteClient(m_clusterOption.Values))
                {
                    return ExecuteCommand(ignite.Start(), readOption.Values, writeOption.Value());
                }
            });
        }

        private int ExecuteCommand(IIgnite ignite, IEnumerable<string> input, string output)
        {
            var compute = ignite.GetCompute();
            foreach (var cacheName in input)
            {
                Console.WriteLine($"Tracking flows in {cacheName}");
                Console.WriteLine($"Compute is {String.Join(",", compute.ClusterGroup.GetNodes().Select(n => n.Id.ToString()))}");

                var flowAnalyzer = new FlowAnalyzer()
                {
                    FrameCacheName = cacheName,
                    FlowCacheName = output,
                    Progress = new ConsoleProgressReport()
                };

                compute.Broadcast(flowAnalyzer);
            }
            return 0;
        }
    }
}