using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tarzan.Nfx.Analyzers.Commands
{
    public class DetectServicesCommand : AbstractCommand
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string Name { get; private set; } = "Detect-Services";

        public DetectServicesCommand(CommandOption clusterOption) : base(clusterOption)
        {
        }

        public override void Configuration(CommandLineApplication command)
        {
            command.Description = "Identifies all flows in the given captures.";
            command.HelpOption("-?|-Help");

            var readOption = command.Option("-FlowCache <CacheName>",
                "A name of cache that contains source flows to be classified. Multiple values can be specified.",
                CommandOptionType.MultipleValue);

            var methodOption = command.Option("-Method <Method>",
                "A name of the classification/detection method.",
                CommandOptionType.MultipleValue);

            command.OnExecute(() =>
            {
                using (var ignite = CreateIgniteClient())
                {
                    return ExecuteCommand(ignite.Start(), readOption.Values, methodOption.Values);
                }
            });
        }

        private int ExecuteCommand(IIgnite ignite, List<string> sources, List<string> methods)
        {
            var compute = ignite.GetCompute();
            foreach (var cacheName in sources)
            {
                var flowCache = ignite.GetOrCreateCache<object, object>(cacheName);
                logger.Info($"Detecting services in flow cache: {cacheName}...");                
                var serviceDetector = new ServiceDetector(flowCache.Name);
                compute.Broadcast(serviceDetector);
                logger.Info($"Detecting services in flow cache: {cacheName} completed.");
            }
            return 0;
        }
    }
}
