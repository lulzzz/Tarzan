using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Netdx.PacketDecoders;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    

    class GlobalIgniteConfiguration
    {
        public static readonly IgniteConfiguration Default = new IgniteConfiguration()
        {
            //BinaryConfiguration = new BinaryConfiguration(typeof(PacketFlowKey), typeof(PacketStream)),
            DiscoverySpi = new TcpDiscoverySpi
            {
                IpFinder = new TcpDiscoveryStaticIpFinder
                {
                    Endpoints = new[] { "127.0.0.1:47500..47520" }
                },
                SocketTimeout = TimeSpan.FromSeconds(0.3)
            },
            CacheConfiguration = new[] {
                    new CacheConfiguration
                    {
                        Name = nameof(FlowCache),
                        CacheMode = CacheMode.Partitioned,
                        Backups = 0
                    }
                }
        };
    }
}
