using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using System;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest
{


    class IgniteConfiguration
    {
        public const string FlowCache = "FlowCache";
        public const string DnsObjectCache = "DnsObjectCache";

        public static readonly Apache.Ignite.Core.IgniteConfiguration Default =
            new Apache.Ignite.Core.IgniteConfiguration()
            {
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        Endpoints = new[] { "127.0.0.1:47500..47520" }
                    },
                    SocketTimeout = TimeSpan.FromSeconds(0.3)
                },
                BinaryConfiguration = new BinaryConfiguration()
                {
                    TypeConfigurations = new[]
                    {
                        new BinaryTypeConfiguration(typeof (DnsObject))
                        {
                            Serializer = new DnsObjectSerializer()
                        }
                    }
                },
                CacheConfiguration = new []
                {
                     new CacheConfiguration(FlowCache)
                     {
                        CacheMode = CacheMode.Partitioned,
                        Backups = 0,
                        QueryEntities = new []
                        {
                            new QueryEntity(typeof(PacketStream))
                            {
                                Fields = new []
                                {
                                    new QueryField(nameof(PacketStream.Protocol), typeof(short)),
                                    new QueryField(nameof(PacketStream.SourceAddress), typeof(byte[])),
                                    new QueryField(nameof(PacketStream.SourcePort), typeof(int)),
                                    new QueryField(nameof(PacketStream.DestinationAddress), typeof(byte[])),
                                    new QueryField(nameof(PacketStream.DestinationPort), typeof(int)),
                                    new QueryField(nameof(PacketStream.FirstSeen), typeof(long)),
                                    new QueryField(nameof(PacketStream.LastSeen), typeof(long)),
                                    new QueryField(nameof(PacketStream.Packets), typeof(int)),
                                    new QueryField(nameof(PacketStream.Octets), typeof(long)),
                                }
                            }
                        },
                    },
                    new CacheConfiguration(DnsObjectCache)
                    {
                        CacheMode = CacheMode.Partitioned,
                        Backups = 0,
                        QueryEntities = new []
                        {
                            new QueryEntity(typeof(DnsObject))
                            {
                                Fields = new []
                                {
                                    new QueryField { Name = nameof(DnsObject.Client), FieldType = typeof(string) },
                                    new QueryField { Name = nameof(DnsObject.DnsAnswer), FieldType = typeof(string) },
                                    new QueryField { Name = nameof(DnsObject.DnsQuery), FieldType = typeof(string) },
                                    new QueryField { Name = nameof(DnsObject.DnsTtl), FieldType = typeof(int) },
                                    new QueryField { Name = nameof(DnsObject.DnsType), FieldType = typeof(string) },
                                    new QueryField { Name = nameof(DnsObject.FlowUid), FieldType = typeof(string) },
                                    new QueryField { Name = nameof(DnsObject.Server), FieldType = typeof(string) },
                                    new QueryField { Name = nameof(DnsObject.Timestamp), FieldType = typeof(long) },
                                    new QueryField { Name = nameof(DnsObject.TransactionId), FieldType = typeof(string) },
                                }
                            }
                        }
                    }
                }
            };
    }
}
