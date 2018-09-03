using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest
{


    class IgniteConfiguration
    {
        public const string FlowCache = "FlowCache";

        public static readonly Apache.Ignite.Core.IgniteConfiguration Default = new Apache.Ignite.Core.IgniteConfiguration()
        {
            BinaryConfiguration = new BinaryConfiguration(typeof(FlowKey), typeof(PacketStream), typeof(Host), typeof(Service)),
            DiscoverySpi = new TcpDiscoverySpi
            {
                IpFinder = new TcpDiscoveryStaticIpFinder
                {
                    Endpoints = new[] { "127.0.0.1:47500..47520" }
                },
                SocketTimeout = TimeSpan.FromSeconds(0.3)
            },
            CacheConfiguration = new[] {
                                new CacheConfiguration(FlowCache, new QueryEntity(typeof(FlowKey)), new QueryEntity(typeof(PacketStream)))
                                {
                                    CacheMode = CacheMode.Partitioned,
                                    Backups = 0
                                }
            }
        };
    }
}
