using System;
using System.Net;

namespace Tarzan.UI.Server.Models
{

    /// <summary>
    /// Represents a single flow record.
    /// </summary>
    public class FlowRecord
    {
        /// <summary>
        /// A unique identifier of the flow record.
        /// </summary>
        public string FlowId { get; set; }
        /// <summary>
        /// Type of transport (or internet) protocol of the flow.
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// The network source address of the flow.
        /// </summary>
        public string SourceAddress { get; set; }
        /// <summary>
        /// Source port (if any) of the flow.
        /// </summary>
        public int SourcePort { get; set; }
        /// <summary>
        /// The network destination address of the flow.
        /// </summary>
        public string DestinationAddress { get; set; }
        /// <summary>
        /// The destination port of the flow.
        /// </summary>
        /// <returns></returns>
        public int DestinationPort { get; set; }
        /// <summary>
        /// Unix time stamp of the start of flow.
        /// </summary>
        public Int64 FirstSeen { get; set; }
        /// <summary>
        /// The unix time stamp of the end of flow.
        /// </summary>
        /// <returns></returns>
        public Int64 LastSeen { get; set; }
        /// <summary>
        /// Number of packets carried by the flow.
        /// </summary>
        public Int32 Packets { get; set; }
        /// <summary>
        /// Total number of octets carried by the flow.
        /// </summary>
        /// <returns></returns>
        public Int64 Octets { get; set; }


        /// <summary>
        /// Creates an empty flow record.
        /// </summary>
        public FlowRecord()
        {

        }
        /// <summary>
        /// Creates new flow record reding values from Cassandra row.null 
        /// </summary>
        /// <param name="row"></param>
        public FlowRecord(Cassandra.Row row)
        {
            FlowId = row.GetValue<Guid>("flowid").ToString();
            Protocol = row.GetValue<string>("protocol");
            var source = row.GetValue<IpEndPoint>("source");
            SourceAddress = source.Address.ToString();
            SourcePort = source.Port;
            var destination = row.GetValue<IpEndPoint>("destination");
            DestinationAddress = destination.Address.ToString();
            DestinationPort = destination.Port;
            FirstSeen = new DateTimeOffset(row.GetValue<DateTime>("firstseen")).ToUnixTimeMilliseconds();
            LastSeen = new DateTimeOffset(row.GetValue<DateTime>("lastseen")).ToUnixTimeMilliseconds();
            Octets = row.GetValue<Int64>("octets");
            Packets = row.GetValue<int>("packets");
        }

        public class IpEndPoint : IPEndPoint
        {
            public IpEndPoint() : base(IPAddress.Any, 0) { }
            public IpEndPoint(IPEndPoint ep) : base(ep.Address, ep.Port) { }
        }
    }
}