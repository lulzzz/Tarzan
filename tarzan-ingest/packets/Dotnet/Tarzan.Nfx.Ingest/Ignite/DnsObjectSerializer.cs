using Apache.Ignite.Core.Binary;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class DnsObjectSerializer : IBinarySerializer
    {
        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            var dns = (DnsObject)obj;

            writer.WriteString(nameof(DnsObject.Client), dns.Client);
            writer.WriteString(nameof(DnsObject.DnsAnswer), dns.DnsAnswer);
            writer.WriteString(nameof(DnsObject.DnsQuery), dns.DnsQuery);
            writer.WriteInt(nameof(DnsObject.DnsTtl), dns.DnsTtl);
            writer.WriteString(nameof(DnsObject.DnsType), dns.DnsType);
            writer.WriteString(nameof(DnsObject.FlowUid), dns.FlowUid);
            writer.WriteString(nameof(DnsObject.Server), dns.Server);
            writer.WriteLong(nameof(DnsObject.Timestamp), dns.Timestamp);
            writer.WriteString(nameof(DnsObject.TransactionId), dns.TransactionId);
        }

        public void ReadBinary(object obj, IBinaryReader reader)
        {
            var dns = (DnsObject)obj;

            dns.Client = reader.ReadString(nameof(DnsObject.Client));
            dns.DnsAnswer = reader.ReadString(nameof(DnsObject.DnsAnswer));
            dns.DnsQuery = reader.ReadString(nameof(DnsObject.DnsQuery));
            dns.DnsTtl = reader.ReadInt(nameof(DnsObject.DnsTtl));
            dns.DnsType = reader.ReadString(nameof(DnsObject.DnsType));
            dns.FlowUid = reader.ReadString(nameof(DnsObject.FlowUid));
            dns.Server = reader.ReadString(nameof(DnsObject.Server) );
            dns.Timestamp = reader.ReadLong(nameof(DnsObject.Timestamp));
            dns.TransactionId = reader.ReadString(nameof(DnsObject.TransactionId));
        }
    }
}
