using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Net;

namespace Tarzan.Nfx.Model
{
    /// <summary>
    /// Represents a single flow record.
    /// </summary>
    [Serializable]
    public partial class FlowData : IBinarizable
    {
        [QuerySqlField]
        public string FlowUid { get; set; }
        [QuerySqlField]
        public string Protocol { get; set; }
        [QuerySqlField]
        public string SourceAddress { get; set; }
        [QuerySqlField]
        public int SourcePort { get; set; }
        [QuerySqlField]
        public string DestinationAddress { get; set; }
        [QuerySqlField]
        public int DestinationPort { get; set; }
        [QuerySqlField]
        public long FirstSeen { get; set; }
        [QuerySqlField]
        public long LastSeen { get; set; }
        [QuerySqlField]
        public int Packets { get; set; }
        [QuerySqlField]
        public long Octets { get; set; }
        [QuerySqlField(IsIndexed = true)]
        public string ServiceName { get; set; }

        public void ReadBinary(IBinaryReader reader)
        {
            FlowUid = reader.ReadString(nameof(FlowUid));
            Protocol = reader.ReadString(nameof(Protocol));
            SourceAddress= reader.ReadString(nameof(SourceAddress));
            SourcePort = reader.ReadInt(nameof(SourcePort));
            DestinationAddress =  reader.ReadString(nameof(DestinationAddress));
            DestinationPort = reader.ReadInt(nameof(DestinationPort));
            FirstSeen = reader.ReadLong(nameof(FirstSeen));
            LastSeen = reader.ReadLong(nameof(LastSeen));
            Packets = reader.ReadInt(nameof(Packets));
            Octets = reader.ReadLong(nameof(Octets));
            ServiceName = reader.ReadString(nameof(ServiceName));
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteString(nameof(FlowUid), FlowUid);
            writer.WriteString(nameof(Protocol), Protocol);
            writer.WriteString(nameof(SourceAddress), SourceAddress);
            writer.WriteInt(nameof(SourcePort), SourcePort);
            writer.WriteString(nameof(DestinationAddress), DestinationAddress);
            writer.WriteInt(nameof(DestinationPort), DestinationPort);
            writer.WriteLong(nameof(FirstSeen), FirstSeen);
            writer.WriteLong(nameof(LastSeen), LastSeen);
            writer.WriteInt(nameof(Packets), Packets);
            writer.WriteLong(nameof(Octets), Octets);
            writer.WriteString(nameof(ServiceName), ServiceName);
        }

        public string ObjectName => $"urn:aff4:flow/{this.FlowUid}";

        public IPAddress SourceIpAddress => IPAddress.Parse(this.SourceAddress);

        public IPAddress DestinationIpAddress => IPAddress.Parse(this.DestinationAddress);
    }
}