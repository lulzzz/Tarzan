using Apache.Ignite.Core;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Net;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.FlowTracker
{
    class Program
    {
        const int DEFAULT_PORT = 47500;
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);
            var clusterArgument = commandLineApplication.Option("-c|--cluster", "Enpoint string of any cluster node.", CommandOptionType.SingleValue);
            var cacheArgument = commandLineApplication.Option("-c|--cache", "Name of the cache with frames to process.", CommandOptionType.MultipleValue);

            commandLineApplication.OnExecute(async () =>
            {
                var ep = IPEndPointExtensions.Parse(clusterArgument.HasValue() ? clusterArgument.Value() : IPAddress.Loopback.ToString(), DEFAULT_PORT);
                var cfg = new IgniteConfiguration
                {
                    PeerAssemblyLoadingMode = Apache.Ignite.Core.Deployment.PeerAssemblyLoadingMode.CurrentAppDomain,
                    DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                    {

                        IpFinder = new TcpDiscoveryStaticIpFinder
                        {
                            Endpoints = new[] { $"{ep.Address}:{(ep.Port != 0 ? ep.Port : DEFAULT_PORT)}" }
                        },
                    }
                };
                Ignition.ClientMode = true;
                using (var client = Ignition.Start(cfg))
                {
                    foreach (var cacheName in cacheArgument.Values)
                    {
                        Console.WriteLine($"Tracking flows in {cacheName}");
                        client.GetCompute().Broadcast(new FlowAnalyzer() { CacheName = cacheName });
                    }
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
