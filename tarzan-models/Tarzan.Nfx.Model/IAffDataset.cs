using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;

namespace Tarzan.Nfx.Model
{
    public interface IAffDataset
    {
        Table<Capture> CaptureTable { get; }
        Table<AffObject> CatalogueTable { get; }
        Table<DnsObject> DnsTable { get; }
        Table<FlowData> FlowTable { get; }
        Table<Host> HostTable { get; }
        Table<HttpObject> HttpTable { get; }
        IMapper Mapper { get; }
        Table<AffStatement> RelationsTable { get; }
        Table<Service> ServiceTable { get; }
        ISession Session { get; }

        void Close();
        void Connect();
    }
}