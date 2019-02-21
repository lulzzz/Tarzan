using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Deployment;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Multicast;
using System;
using System.Collections.Generic;

namespace StreamingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ignite = Ignition.Start(GetDefaultConfiguration()))
            {
                var cache = ignite.GetOrCreateCache<string, string>("Cache");
                using (var dataStreamer = cache.Ignite.GetDataStreamer<string, string>(cache.Name))
                {
                    dataStreamer.AllowOverwrite = true;
                    dataStreamer.Receiver = new PacketFlowVisitor2();
                    

                    for (int i = 0; i < 1000; i++)
                    {
                        dataStreamer.AddData(i.ToString(), ""); // new Artifact { Id = i.ToString(), PayloadBin = new byte[i] });
                    }

                    for (int i = 0; i < 1000; i++)
                    {
                        dataStreamer.AddData(i.ToString(), ""); // new Artifact { Id = i.ToString(), PayloadBin = new byte[i] });
                    }

                    dataStreamer.Flush();
                }
            }
        }
        private static IgniteConfiguration GetDefaultConfiguration()
        {
            var cfg = new IgniteConfiguration
            {
                JvmOptions = new[] {
                                     "-XX:+AlwaysPreTouch",
                                     "-XX:+UseG1GC",
                                     "-XX:+ScavengeBeforeFullGC",
                                     "-XX:+DisableExplicitGC",
                },
                PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,

                DataStorageConfiguration = new DataStorageConfiguration(),
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryMulticastIpFinder
                    {
                        MulticastGroup = "228.10.10.157",
                        Endpoints = new[] { "127.0.0.1:47500" }
                    }
                }
            };

            return cfg;
        }
    }
    public class PacketFlowVisitor2 : IStreamReceiver<string, string>
    {
        public void Receive(ICache<string, string> cache, ICollection<ICacheEntry<string, string>> entries)
        {
            Console.Write('.');
        }
    }
}
