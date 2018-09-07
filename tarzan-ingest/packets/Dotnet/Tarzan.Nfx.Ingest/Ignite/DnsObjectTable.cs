using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class DnsObjectTable : ICacheFactory<string, DnsObject>
    {
        internal static readonly IBinarySerializer m_serializer = new DnsObjectSerializer();
        internal static readonly CacheConfiguration m_cacheConfiguration =
            new CacheConfiguration(nameof(DnsObjectTable))
            {
                CacheMode = CacheMode.Partitioned,
                Backups = 0,
                QueryEntities = new[]
                {
                    new QueryEntity(typeof(DnsObject))
                    {
                        Fields = new []
                        {
                            new QueryField { Name = nameof(DnsObject.Timestamp), FieldType = typeof(long) },
                            new QueryField { Name = nameof(DnsObject.FlowUid), FieldType = typeof(string) },
                            new QueryField { Name = nameof(DnsObject.TransactionId), FieldType = typeof(string) },
                            
                            new QueryField { Name = nameof(DnsObject.Client), FieldType = typeof(string) },
                            new QueryField { Name = nameof(DnsObject.Server), FieldType = typeof(string) },
                            new QueryField { Name = nameof(DnsObject.DnsQuery), FieldType = typeof(string) },
                            new QueryField { Name = nameof(DnsObject.DnsType), FieldType = typeof(string) },
                            new QueryField { Name = nameof(DnsObject.DnsAnswer), FieldType = typeof(string) },
                            new QueryField { Name = nameof(DnsObject.DnsTtl), FieldType = typeof(int) },
                        }
                    }
                }
            };
        private IIgnite m_ignite;

        public DnsObjectTable(IIgnite m_ignite)
        {
            this.m_ignite = m_ignite;
        }

        public IBinarySerializer Serializer => m_serializer;

        public CacheConfiguration CacheConfiguration => m_cacheConfiguration;

        public ICache<string, DnsObject> GetCache()
        {
            return m_ignite.GetCache<string, DnsObject>(nameof(DnsObjectTable));
        }

        public ICache<string, DnsObject> GetOrCreateCache()
        {
            return m_ignite.GetOrCreateCache<string, DnsObject>(nameof(DnsObjectTable));
        }
    }
}
