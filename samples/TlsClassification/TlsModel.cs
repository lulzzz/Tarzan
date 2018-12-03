using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public enum TlsDirection { ClientServer, ServerClient }
    public class TlsConversationContext : DbContext
    {

        public TlsConversationContext(DbContextOptions<TlsConversationContext> options) : base(options) { }
        public static TlsConversationContext CreateInMemory()
        {
            DbContextOptions<TlsConversationContext> options;
            var builder = new DbContextOptionsBuilder<TlsConversationContext>();
            builder.UseInMemoryDatabase("tlsflows");
            options = builder.Options;
            return new TlsConversationContext(options);
        }

        public DbSet<TlsConversationModel> Conversations { get; set; }
        public DbSet<TlsRecordModel> TlsRecords { get; set; }
        public DbSet<TcpSegmentModel> TcpSegments { get; set; }
    }

    public class TlsConversationModel
    {
        [Key]
        public string ConversationKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public string Version { get; set; }
        public string SessionId { get; set; }

        public string ClientRandom { get; set; }
        public string ClientCipherSuites { get; set; }
        public string ClientExtensions { get; set; }
        public List<TlsCertificateModel> ClientCertificates { get; set; }
        
        public string ServerRandom { get; set; }
        public string ServerCipherSuite { get; set; }
        public string ServerExtensions { get; set; }
        public List<TlsCertificateModel> ServerCertificates { get; set; }
        
        public List<TlsRecordModel> Records { get; set; }
    }

    public class TlsCertificateModel
    {
        [Key]
        public string SerialNumber { get; set; }

        public string SubjectName { get; set; }
        public string IssuerName { get;  set; }
        public DateTime NotBefore { get; internal set; }
        public DateTime NotAfter { get; internal set; }
    }

    public class TlsRecordModel
    {
        [Key]
        public int RecordId { get; set; }

        public TlsDirection Direction { get; set; }
        public TimeSpan TimeOffset { get; set; }
        public int Length { get; set; }
        public List<TcpSegmentModel> Segments { get; set; }
    }

    public class TcpSegmentModel
    {
        [Key]
        public int PacketId { get; set; }

        public TimeSpan TimeOffset { get; set; }
        public int Length { get; set; }
        public string Flags { get; set; }
        public int Window { get; set; }
    }
}
