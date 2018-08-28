using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using System;
using System.Net;

namespace Tarzan.Nfx.Model.Cassandra
{
    /// <summary>
    /// This is a top level object that provides access to the AFF Cassandra Database.
    /// </summary>
    public class AffDataset : IAffDataset
    {
        private readonly IPEndPoint m_endpoint;
        private readonly string m_keyspace;
        private IMapper m_mapper;

        /// <summary>
        /// Gets the current session used by this <see cref="AffDataset"/> object. Session object enables to 
        /// manipulate with database connection.
        /// </summary>
        public ISession Session { get; private set; }

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
            Session.Cluster.Shutdown();
        }

        /// <summary>
        /// Connects the <see cref="AffDataset"/> to the Casandra cluster. 
        /// </summary>
        public void Connect()
        {
            ModelMapping.AutoRegister(MappingConfiguration.Global);
            var cluster = Cluster.Builder().AddContactPoint(m_endpoint).WithDefaultKeyspace(m_keyspace).Build();
            Session = cluster.ConnectAndCreateDefaultKeyspaceIfNotExists();
            CreateTables();
        }

        /// <summary>
        /// Initializes table objects and creates all tables in the database if they do not exist.
        /// </summary>
        private void CreateTables()
        {

            CaptureTable = new Table<Model.Capture>(Session);
            CaptureTable.CreateIfNotExists();
            FlowTable = new Table<Model.PacketFlow>(Session);
            FlowTable.CreateIfNotExists();
            HostTable = new Table<Model.Host>(Session);
            HostTable.CreateIfNotExists();
            ServiceTable = new Table<Model.Service>(Session);
            ServiceTable.CreateIfNotExists();
            DnsTable = new Table<Model.DnsObject>(Session);
            DnsTable.CreateIfNotExists();
            HttpTable = new Table<Model.HttpObject>(Session);
            HttpTable.CreateIfNotExists();
            CatalogueTable = new Table<Model.AffObject>(Session);
            CatalogueTable.CreateIfNotExists();
            RelationsTable = new Table<Model.AffStatement>(Session);
            RelationsTable.CreateIfNotExists();
        }

        /// <summary>
        /// Gets the mapper object for the current <see cref="AffDataset"/> object. 
        /// </summary>
        public IMapper Mapper
        {
            get
            {
                if (Session == null) throw new InvalidOperationException("Invalid session object.");
                if (m_mapper == null) m_mapper = new Mapper(Session);
                return m_mapper;
            }
        }

        public Table<PacketFlow> FlowTable { get; private set; }
        public Table<Host> HostTable { get; private set; }
        public Table<Service> ServiceTable { get; private set; }
        public Table<DnsObject> DnsTable { get; private set; }
        public Table<HttpObject> HttpTable { get; private set; }
        public Table<AffObject> CatalogueTable { get; private set; }
        public Table<AffStatement> RelationsTable { get; private set; }
        public Table<Capture> CaptureTable { get; private set; }


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
