using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Communication;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using System;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest
{


    class IgniteConfiguration
    {
        public static readonly Apache.Ignite.Core.IgniteConfiguration Default =
            new Apache.Ignite.Core.IgniteConfiguration()
            {
                JvmOptions = new [] { "-Xms2g", "-Xmx2g", "-XX:+AlwaysPreTouch", "-XX:+UseG1GC", "-XX:+ScavengeBeforeFullGC", "-XX:+DisableExplicitGC", "-XX:MaxDirectMemorySize=256m" },
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        Endpoints = new[] { "127.0.0.1:47500..47520" }
                    },
                    SocketTimeout = TimeSpan.FromSeconds(0.3)
                },
                CommunicationSpi = new TcpCommunicationSpi
                {
                    MessageQueueLimit = 1024,
                },
                BinaryConfiguration = new BinaryConfiguration()
                {
                    TypeConfigurations = new[]
                    {
                        new BinaryTypeConfiguration(typeof(PacketStream))
                        {
                            Serializer = FlowTable.m_serializer
                        },
                        new BinaryTypeConfiguration(typeof(DnsObject))
                        {
                            Serializer = DnsObjectTable.m_serializer,
                        },
                        new BinaryTypeConfiguration(typeof(HttpObject))
                        {
                            Serializer = HttpObjectTable.m_serializer
                        }
                    }
                },
                CacheConfiguration = new[]
                {
                    FlowTable.m_cacheConfiguration,
                    DnsObjectTable.m_cacheConfiguration,
                    HttpObjectTable.m_cacheConfiguration
                }
            };
        
    }
}
