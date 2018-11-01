using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Commands
{
    class TrackFlowsCommand : AbstractCommand
    {
        public TrackFlowsCommand(CommandOption clusterOption)  : base(clusterOption)
        {
            
        }

        public static string Name { get; private set; } = "Track-Flows";

        public override void Configuration(CommandLineApplication command)
        {
            command.Description = "Identifies all flows in the given captures.";
            command.HelpOption("-?|-Help");

            var readOption = command.Option("-PacketCache <CacheName>",
                        "A name of cache that contains source frames. Multiple values can be specified.",
                        CommandOptionType.MultipleValue);

            var writeOption = command.Option("-WriteTo <CacheName>",
                        "A Possibly fresh name of the cache that will be populated with identified flows.",
                        CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                using (var ignite = CreateIgniteClient())
                {
                    return ExecuteCommand(ignite.Start(), readOption.Values, writeOption.Value());
                }
            });
        }

        private int ExecuteCommand(IIgnite ignite, IEnumerable<string> input, string output)
        {
            var compute = ignite.GetCompute();   
            var flowCache = ignite.GetOrCreateCache<FlowKey, FlowData>(output);
            foreach (var cacheName in input)
            {
                var frameCache = ignite.GetOrCreateCache<object, object>(cacheName);
                Console.WriteLine($"Tracking flows in {cacheName}");
                Console.WriteLine($"Compute is {String.Join(",", compute.ClusterGroup.GetNodes().Select(n => n.Id.ToString()))}");

                var flowAnalyzer = new FlowAnalyzer()
                {
                    FrameCacheName = cacheName,
                    FlowCacheName = output,
                    Progress = new ConsoleProgressReport()
                };

                compute.Broadcast(flowAnalyzer);
                Console.WriteLine($"Processed frames={frameCache.GetSize()}, flows={flowCache.GetSize()}.");
            }
            return 0;
        }
    }
}