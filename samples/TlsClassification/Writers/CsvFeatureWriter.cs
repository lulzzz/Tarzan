using CsvHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tarzan.Nfx.Samples.TlsClassification.Writers
{
    class CsvFeatureWriter
    {
        const int RecordCount = 10;
        const int SegmentCount = 5;
        public static void WriteCsv(string filename, TlsConversationContext modelContext)
        {
            dynamic CreateRow(TlsConversationModel model)
            {
                                                    
                dynamic record = new ExpandoObject();
                record.ConversationKey = model.ConversationKey;
                record.Timestamp = model.Timestamp;
                record.Version = model.Version;
                record.SessionId = model.SessionId;
                record.ClientCipherSuites = model.ClientCipherSuites;
                record.ClientExtensions = model.ClientExtensions;
                record.ServerCipherSuite = model.ServerCipherSuite;
                record.ServerExtensions = model.ServerExtensions;
                record.ServerCertificates = model.ServerCertificates;

                var tlsRecords = model.Records.OrderBy(r => r.RecordId).ToList();
                var expando = record as IDictionary<String, Object>;
                for(int index = 0; index < RecordCount; index++)
                {
                    var tlsrecord = index < tlsRecords.Count ? tlsRecords[index] : new TlsRecordModel { };
                    var recordString = $"Records[{index}]"; 
                    expando[$"{recordString}.Length"] = tlsrecord.Direction == TlsDirection.ClientServer ? -tlsrecord.Length : tlsrecord.Length;
                    expando[$"{recordString}.TimeOffset"] = tlsrecord.TimeOffset.TotalMilliseconds;

                    for (var segmentIndex = 0; segmentIndex < SegmentCount; segmentIndex++)
                    {
                        var tcpSegment = tlsrecord?.Segments != null && segmentIndex < tlsrecord.Segments.Count ? tlsrecord.Segments[segmentIndex] : new TcpSegmentModel { };
                        expando[$"{recordString}.Segments[{segmentIndex}].Flags"] = tcpSegment.Flags;
                        expando[$"{recordString}.Segments[{segmentIndex}].Length"] = tcpSegment.Length;
                        expando[$"{recordString}.Segments[{segmentIndex}].TimeOffset"] = tcpSegment.TimeOffset.TotalMilliseconds;
                        expando[$"{recordString}.Segments[{segmentIndex}].Window"] = tcpSegment.Window;
                    }
                }

                return record;
            }


            using (var textWriter = new StreamWriter(filename))
            {

                var rows = modelContext.Conversations.Select(CreateRow);
                var csv = new CsvWriter(textWriter);
                csv.WriteRecords(rows);
            }
        }
    }
}
