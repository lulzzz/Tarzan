using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class HttpObjectTable : ICacheFactory<string, HttpObject>
    {
        internal static readonly IBinarySerializer m_serializer = new HttpObjectSerializer();
        internal static readonly CacheConfiguration m_cacheConfiguration =
            new CacheConfiguration(nameof(HttpObjectTable))
            {
                CacheMode = CacheMode.Partitioned,
                Backups = 0,
                QueryEntities = new[]
                {
                    new QueryEntity(typeof(HttpObject))
                    {
                        Fields = new []
                        {
                            new QueryField { Name = nameof(HttpObject.Timestamp), FieldType = typeof(long) },
                            new QueryField { Name = nameof(HttpObject.FlowUid), FieldType = typeof(string) },
                            new QueryField { Name = nameof(HttpObject.ObjectIndex), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.Client), FieldType = typeof(string) },
                            new QueryField { Name = nameof(HttpObject.Server), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.Method), FieldType = typeof(string) },
                            new QueryField { Name = nameof(HttpObject.Version), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.Host), FieldType = typeof(string) },
                            new QueryField { Name = nameof(HttpObject.Uri), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.Referrer), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.RequestBodyLength), FieldType = typeof(int) },
                            new QueryField { Name = nameof(HttpObject.RequestContentType), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.UserAgent), FieldType = typeof(string) },
                            new QueryField { Name = nameof(HttpObject.Username), FieldType = typeof(string) },
                            new QueryField { Name = nameof(HttpObject.Password), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.StatusCode), FieldType = typeof(string) },
                            new QueryField { Name = nameof(HttpObject.StatusMessage), FieldType = typeof(string) },

                            new QueryField { Name = nameof(HttpObject.ResponseBodyLength), FieldType = typeof(int) },
                            new QueryField { Name = nameof(HttpObject.ResponseContentType), FieldType = typeof(string) },
                        }
                    }
                }
            };
        private IIgnite m_ignite;

        public HttpObjectTable(IIgnite m_ignite)
        {
            this.m_ignite = m_ignite;
        }

        public IBinarySerializer Serializer => m_serializer;

        public CacheConfiguration CacheConfiguration => m_cacheConfiguration;

        public ICache<string, HttpObject> GetCache()
        {
            return m_ignite.GetCache<string, HttpObject>(nameof(HttpObjectTable));
        }

        public ICache<string, HttpObject> GetOrCreateCache()
        {
            return m_ignite.GetOrCreateCache<string, HttpObject>(nameof(HttpObjectTable));
        }
    }
}
