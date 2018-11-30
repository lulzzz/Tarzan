using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public enum TlsDirection { ClientServer, ServerClient }
    public class TlsFlowContext : DbContext
    {
        public DbSet<TlsFlowModel> TlsFlows { get; set; }
        public DbSet<TlsRecordModel> TlsRecords { get; set; }
        public DbSet<TcpSegmentModel> TcpSegments { get; set; }
    }

    public class TlsFlowModel
    {
        public FlowKey Key { get; set; }
        public string Version { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public string SessionId { get; set; }
        public string   ClientRandom { get; set; }
        public string[] ClientExtensions { get; set; }
        public string[] ClientCertificates { get; set; }
        public string[] ClientCipherSuites { get; set; }
        

        public string   ServerRandom { get; set; }
        public string   ServerCipherSuite { get; set; }
        public string[] ServerCertificates { get; set; }
        public string[] ServerExtensions { get; set; }

        public virtual List<TlsRecordModel> Records { get; set; }
    }

    public class TlsRecordModel
    {
        public int RecordNumber { get; set; }
        public TlsDirection Direction { get; set; }
        public TimeSpan TimeOffset { get; set; }
        public int Length { get; set; }
        public virtual List<TcpSegmentModel> Segments { get; set; }
    }

    public class TcpSegmentModel
    {
        public int PacketNumber { get; set; }
        public TimeSpan TimeOffset { get; set; }
        public int Length { get; set; }
        public string Flags { get; set; }
        public int Window { get; set; }

    }
}
