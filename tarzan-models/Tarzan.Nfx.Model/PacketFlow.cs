using System;
using System.Net;
using Apache.Ignite.Core.Binary;
using Cassandra.Mapping;
using Newtonsoft.Json;

namespace Tarzan.Nfx.Model
{
    /// <summary>
    /// Represents a single flow record.
    /// </summary>
    public partial class PacketFlow : IBinarizable
    {
        public static string CACHE_NAME = "flowtable";


        public static Map<PacketFlow> Mapping =>
            new Map<PacketFlow>()
                .TableName(Pluralizer.Pluralize(nameof(PacketFlow)))
                .PartitionKey(x => x.FlowUid)
                .Column(f => f.SourceIpAddress, cc => cc.Ignore())
                .Column(f => f.DestinationIpAddress, cc => cc.Ignore())
                .Column(f => f.__isset, cc => cc.Ignore())
                .Column(f => f.Protocol, cc => cc.WithSecondaryIndex())
                .Column(f => f.SourceAddress, cc => cc.WithSecondaryIndex())
                .Column(f => f.SourcePort, cc => cc.WithSecondaryIndex())
                .Column(f => f.DestinationAddress, cc => cc.WithSecondaryIndex())
                .Column(f => f.DestinationPort, cc => cc.WithSecondaryIndex());

        public string ObjectName => $"urn:aff4:flow/{this.FlowUid}";
        [JsonIgnore]
        public IPAddress SourceIpAddress => IPAddress.Parse(this.SourceAddress);
        [JsonIgnore]
        public IPAddress DestinationIpAddress => IPAddress.Parse(this.DestinationAddress);

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
    }
}