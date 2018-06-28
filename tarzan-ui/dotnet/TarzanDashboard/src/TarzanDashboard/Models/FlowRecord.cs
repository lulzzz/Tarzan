using System;
using System.Net;

namespace Tarzan.UI.Server.Models
{
    public class FlowRecord
    {
        public int Id { get; set; }
        public string Protocol { get; set; }
        public string SourceAddress { get; set; }
        public int SourcePort {get; set; }
        public string DestinationAddress { get; set; }
        public int DestinationPort { get; set; }
        public string FirstSeen { get; set; }
        public string LastSeen { get; set; }
        public Int32 Packets { get; set; }
        public Int64 Octets { get; set; }


        public FlowRecord()
        {

        }
        public FlowRecord(Cassandra.Row row)
        {
            Id = row.GetValue<int>("id");
            Protocol = row.GetValue<string>("protocol");
            SourceAddress = row.GetValue<IPAddress>("sourceaddress").ToString();
            SourcePort = row.GetValue<int>("sourceport");
            DestinationAddress = row.GetValue<IPAddress>("destinationaddress").ToString();
            DestinationPort = row.GetValue<int>("destinationport");
            FirstSeen = row.GetValue<DateTime>("firstseen").ToString();
            LastSeen = row.GetValue<DateTime>("lastseen").ToString();
            Octets = row.GetValue<Int64>("octets");
            Packets = row.GetValue<int>("packets");
        }
    }
}