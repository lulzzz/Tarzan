using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Tarzan.Nfx.Model.Cassandra
{
    /// <summary>
    /// This is a top level object that provides access to the AFF Cassandra Database.
    /// </summary>
    public class AffDataset : IAffDataset
    {
        private IPEndPoint m_endpoint;
        private string m_keyspace;
        private ISession m_session;
        private IMapper m_mapper;

        private Table<Capture> m_captureTable;
        private Table<PacketFlow> m_flowTable;
        private Table<Host> m_hostTable;
        private Table<Service> m_serviceTable;
        private Table<DnsObject> m_dnsTable;
        private Table<HttpObject> m_httpTable;
        private Table<AffObject> m_catalogueTable;
        private Table<AffStatement> m_relationsTable;

        /// <summary>
        /// Gets the current session used by this <see cref="AffDataset"/> object. Session object enables to 
        /// manipulate with database connection.
        /// </summary>
        public ISession Session => m_session;

        public AffDataset(IPEndPoint endpoint, string keyspace)
        {
            m_endpoint = endpoint;
            m_keyspace = keyspace;
        }

        /// <summary>
        /// Close connection to the Cassandra cluster.
        /// </summary>
        public void Close()
        {
            m_session.Cluster.Shutdown();
        }

        /// <summary>
        /// Connects the <see cref="AffDataset"/> to the Casandra cluster. 
        /// </summary>
        public void Connect()
        {
            ModelMapping.AutoRegister(MappingConfiguration.Global);
            var cluster = Cluster.Builder().AddContactPoint(m_endpoint).WithDefaultKeyspace(m_keyspace).Build();
            m_session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            CreateTables();
        }

        /// <summary>
        /// Initializes table objects and creates all tables in the database if they do not exist.
        /// </summary>
        private void CreateTables()
        {

            m_captureTable = new Table<Model.Capture>(m_session);
            m_captureTable.CreateIfNotExists();
            m_flowTable = new Table<Model.PacketFlow>(m_session);
            m_flowTable.CreateIfNotExists();
            m_hostTable = new Table<Model.Host>(m_session);
            m_hostTable.CreateIfNotExists();
            m_serviceTable = new Table<Model.Service>(m_session);
            m_serviceTable.CreateIfNotExists();
            m_dnsTable = new Table<Model.DnsObject>(m_session);
            m_dnsTable.CreateIfNotExists();
            m_httpTable = new Table<Model.HttpObject>(m_session);
            m_httpTable.CreateIfNotExists();
            m_catalogueTable = new Table<Model.AffObject>(m_session);
            m_catalogueTable.CreateIfNotExists();
            m_relationsTable = new Table<Model.AffStatement>(m_session);
            m_relationsTable.CreateIfNotExists();
        }

        /// <summary>
        /// Gets the mapper object for the current <see cref="AffDataset"/> object. 
        /// </summary>
        public IMapper Mapper
        {
            get
            {
                if (m_session == null) throw new InvalidOperationException("Invalid session object.");
                if (m_mapper == null) m_mapper = new Mapper(m_session);
                return m_mapper;
            }
        }

        public Table<PacketFlow> FlowTable { get => m_flowTable;  }
        public Table<Host> HostTable { get => m_hostTable;  }
        public Table<Service> ServiceTable { get => m_serviceTable;  }
        public Table<DnsObject> DnsTable { get => m_dnsTable;  }
        public Table<HttpObject> HttpTable { get => m_httpTable; }
        public Table<AffObject> CatalogueTable { get => m_catalogueTable;  }
        public Table<AffStatement> RelationsTable { get => m_relationsTable; }
        public Table<Capture> CaptureTable { get => m_captureTable; }


        /// <summary>
        /// Deletes the entire keyspace in the Cassandra cluster. 
        /// </summary>
        /// <param name="endpoint">Contact point of the cluster.</param>
        /// <param name="keyspace">Name of the keyspace.</param>
        public static void DeleteKeyspace(IPEndPoint endpoint, string keyspace)
        {
            var cluster = Cluster.Builder().AddContactPoint(endpoint).Build();
            using (var session = cluster.Connect())
            {
                session.Execute($"DROP KEYSPACE IF EXISTS {keyspace}");
            }
        }
    }
}
