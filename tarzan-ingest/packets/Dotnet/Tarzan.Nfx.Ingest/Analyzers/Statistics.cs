using Apache.Ignite.Core;
using Apache.Ignite.Linq;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public class Statistics
    {
        private readonly IIgnite m_ignite;

        public Statistics(IIgnite ignite)
        {
            m_ignite = ignite;
        }

        public IEnumerable<Host> GetHosts()
        {
            var queryable = m_ignite.GetCache<FlowKey, PacketStream>(IgniteConfiguration.FlowCache); //.AsCacheQueryable();

            var srcHosts = queryable.GroupBy(x => x.Key.SourceEndpoint.Address).Select(t =>
                new Model.Host { Address = t.Key.ToString(), UpFlows = t.Count(), PacketsSent = t.Sum(p => p.Value.Packets), OctetsSent = t.Sum(p => p.Value.Octets) });

            var dstHosts = queryable.GroupBy(x => x.Key.DestinationEndpoint.Address).Select(t =>
                new Model.Host { Address = t.Key.ToString(), DownFlows = t.Count(), PacketsRecv = t.Sum(p => p.Value.Packets), OctetsRecv = t.Sum(p => p.Value.Octets) });

            var hosts = srcHosts.Join(dstHosts, x => x.Address, x => x.Address, (x, y) =>
                      new Model.Host { Address = x.Address, UpFlows = x.UpFlows, PacketsSent = x.PacketsSent, OctetsSent = x.OctetsSent,
                          DownFlows = y.DownFlows, PacketsRecv = y.PacketsRecv, OctetsRecv = y.OctetsRecv });

            return hosts.ToList();
        }


        public IEnumerable<Service> GetServices()
        {
            var queryable = m_ignite.GetCache<FlowKey, PacketStream>(IgniteConfiguration.FlowCache); //.AsCacheQueryable();
            var services = queryable
                            .GroupBy(f => f.Value.ServiceName)
                            .Select(f =>
                                new Model.Service
                                {
                                    ServiceName = f.Key,
                                    Flows = f.Count(),
                                    Packets = f.Sum(x => x.Value.Packets),
                                    MaxPackets = f.Max(x => x.Value.Packets),
                                    MinPackets = f.Min(x => x.Value.Packets),
                                    Octets = f.Sum(x => x.Value.Octets),
                                    MaxOctets = f.Max(x => x.Value.Octets),
                                    MinOctets = f.Min(x => x.Value.Octets),
                                    AvgDuration = (long)f.Average(x => x.Value.LastSeen - x.Value.FirstSeen),
                                    MaxDuration = f.Max(x => x.Value.LastSeen - x.Value.FirstSeen),
                                    MinDuration = f.Min(x => x.Value.LastSeen - x.Value.FirstSeen),
                                });
            return services.ToList();
        }
    }
}
