using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers.Commands
{
    class TrackFlowsCommand : AbstractCommand
    {
        private static NLog.Logger m_logger = NLog.LogManager.GetCurrentClassLogger();
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

        private int ExecuteCommand(IIgnite ignite, IEnumerable<string> sourceFrameCacheNames, string flowCacheName)
        {
            
            var compute = ignite.GetCompute();   
            var flowCache = CacheFactory.GetOrCreateFlowCache(ignite, flowCacheName);
            foreach (var cacheName in sourceFrameCacheNames)
            {
                var frameCache = ignite.GetOrCreateCache<object, object>(cacheName);
                m_logger.Info($"Tracking flows in frame cache:{cacheName}...");
                var flowAnalyzer = new FlowAnalyzer()
                {
                    FrameCacheName = cacheName,
                    FlowCacheName = flowCacheName,
                };

                compute.Broadcast(flowAnalyzer);
                m_logger.Info($"Tracking flows in frame cache:{cacheName} done, frames={frameCache.GetSize()}, flows={flowCache.GetSize()}.");
            }
            return 0;
        }
    }
}