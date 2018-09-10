using Apache.Ignite.Core;
using Apache.Ignite.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public class Statistics
    {
        private readonly IIgnite m_ignite;
        private readonly PacketFlowTable m_flowCache;

        public Statistics(IIgnite ignite)
        {
            m_ignite = ignite;
            m_flowCache = new PacketFlowTable(ignite);
        }
                                            
        public IEnumerable<Host> GetHosts()
        {
            var queryable = m_flowCache.GetCache().AsCacheQueryable();
            var srcHosts = queryable.GroupBy(x => x.Value.SourceAddress).Select(group =>
                new
                {
                    Address = group.Key,
                    UpFlows = group.Count(),
                    PacketsSent = group.Sum(p => p.Value.Packets),
                    OctetsSent = group.Sum(p => p.Value.Octets)
                });

            
            var dstHosts = queryable.GroupBy(x => x.Value.DestinationAddress).Select(group =>
                new
                {
                    Address = group.Key,
                    DownFlows = group.Count(),
                    PacketsRecv = group.Sum(p => p.Value.Packets),
                    OctetsRecv = group.Sum(p => p.Value.Octets)
                });

            return srcHosts.ToList().Join(inner: dstHosts.ToList(),
                outerKeySelector: x => x.Address.ToString(),
                innerKeySelector: y => y.Address.ToString(),
                resultSelector: (x, y) =>
                      new Model.Host
                      {
                          Address = x.Address.ToString(),
                          UpFlows = x.UpFlows,
                          PacketsSent = x.PacketsSent,
                          OctetsSent = x.OctetsSent,
                          DownFlows = y.DownFlows,
                          PacketsRecv = y.PacketsRecv,
                          OctetsRecv = y.OctetsRecv
                      }
                );
        }

        class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null)
                {
                    return left == right;
                }
                return left.SequenceEqual(right);
            }
            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Sum(b => b);
            }
        }


        public IEnumerable<Service> GetServices()
        {
            var queryable = m_flowCache.GetCache().AsCacheQueryable();
            var services = queryable
                            .GroupBy(f => f.Value.ServiceName)
                            .Select(f =>
                                new 
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
            // nned to do this in this way as creating and initializing Service type is not directly supported :(
            return services.ToList().Select(x=> new Service
            {
                ServiceName = x.ServiceName,
                Flows = x.Flows,
                Packets = x.Packets,
                MaxPackets = x.MaxPackets,
                MinPackets = x.MinPackets,
                Octets = x.Octets,
                MaxOctets = x.MaxOctets,
                MinOctets = x.MinOctets,
                AvgDuration = x.AvgDuration,
                MaxDuration = x.MaxDuration,
                MinDuration = x.MinDuration
            });
        }
    }
}
