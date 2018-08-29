using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Log;
using Apache.Ignite.Core.Resource;
using Microsoft.Extensions.CommandLineUtils;
using Netdx.PacketDecoders;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tarzan.Nfx.Ingest.Analyzers;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    partial class TrackFlows
    {
        enum RunMode { Ignite, Parallel, Sequential }
        public static string Name => "track-flows";
        public static Action<CommandLineApplication> Configuration =>
            (CommandLineApplication target) =>
            {
                target.Description = "Track flows and write flow records to the specified output.";

                var optionInputFile = target.Option("-file", "Read packet data from specified PCAP file. The file should have any supported capture file format (including gzipped files).", CommandOptionType.SingleValue);
                var optionInputFolder = target.Option("-folder", "Read packet data from PCAP files in the specified folder.", CommandOptionType.SingleValue);

                var optionCassandra = target.Option("-cassandra", "Specifies address of the Cassandra DB node to store flow records.", CommandOptionType.SingleValue);
                var optionKeyspace = target.Option("-namespace", "Specifies the keyspace in Cassandra DB.", CommandOptionType.SingleValue);
                var optionCreate = target.Option("-create", "Creates/initializes the keyspace in Cassandra DB. The existing keyspace will be deleted.", CommandOptionType.NoValue);
                var optionMode = target.Option("-runAs", "Specifies how the ingestor will run. Possible values are ignite, parallel, sequential. Default is ignite.", CommandOptionType.SingleValue);

                IList<FileInfo> GetFileList()
                {
                    var fileList = new List<FileInfo>();
                    if (optionInputFolder.HasValue())
                    {
                        var dir = new DirectoryInfo(optionInputFolder.Value());
                        foreach (var fileinfo in dir.EnumerateFiles("*.?cap"))
                        {
                            var inputDevice = new CaptureFileReaderDevice(fileinfo.FullName);
                            fileList.Add(fileinfo);
                        }
                    }
                    if (optionInputFile.HasValue())
                    {
                        var fileinfo = new FileInfo(optionInputFile.Value());
                        fileList.Add(fileinfo);
                    }
                    return fileList;
                }

                target.OnExecute(() =>
                {

                    var fileList = GetFileList();
                    var runMode = RunMode.Ignite;
                    Enum.TryParse(optionMode.Value(), true, out runMode);

                    if (fileList.Count == 0)
                    {
                        throw new ArgumentException("At least one source file has to be specified.");
                    }

                    // initialize Cassandra datastore:
                    if (!optionKeyspace.HasValue())
                    {
                        throw new ArgumentException("Keyspace is not specified.");
                    }

                    var cassandraWriter = new CassandraWriter(IPEndPoint.Parse(optionCassandra.Value() ?? "localhost:9042", 9042), optionKeyspace.Value());

                    if (optionCreate.HasValue())
                    {
                        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Deleting keyspace '{optionKeyspace.Value()}'...");
                        cassandraWriter.DeleteKeyspace();
                    }

                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Initializing keyspace '{optionKeyspace.Value()}'...");
                    cassandraWriter.Initialize();

                    // run processing pipeline:

                    switch (runMode)
                    {
                        case RunMode.Ignite:
                            RunAsIgnite(cassandraWriter, fileList.Select(x => x.FullName));
                            break;
                        case RunMode.Parallel:
                            RunAsParallel(cassandraWriter, fileList.Select(x => x.FullName));
                            break;
                        case RunMode.Sequential:
                            RunAsSequential(cassandraWriter, fileList.Select(x => x.FullName));
                            break;
                    }

                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: All Done. Closing connection to Cassandra..");
                    cassandraWriter.Shutdown();
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Ok.");
                    return 0;
                });
            };


        static void RunAsParallel(CassandraWriter cassandraWriter, IEnumerable<string> fileList)
        {
            throw new NotImplementedException();
        }

        static void RunAsSequential(CassandraWriter cassandraWriter, IEnumerable<string> fileList)
        {
            throw new NotImplementedException();
        }

        static void RunAsIgnite(CassandraWriter cassandraWriter, IEnumerable<string> fileList)
        {
            using (var ignite = Ignition.Start(GlobalIgniteConfiguration.Default))
            {
                var flowCache = new FlowCache(ignite);

                var compute = ignite.GetCluster().GetCompute();
                compute.Run(fileList.Select(x => new TrackFlowComputeAction { FileName = x }));

                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Total flows {flowCache.Count()}");

                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Detecting services of flows...");
                compute.Broadcast(new ServiceDetector());

                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Inserting flows into Cassandra...");
                compute.Broadcast(new WriteFlowsToCassandra(cassandraWriter.Endpoint, cassandraWriter.Keyspace));
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: DONE!");

                // STATISTICS:
                var stats = new Statistics(flowCache);
                cassandraWriter.WriteHosts(stats.GetHosts());

                cassandraWriter.WriteServices(stats.GetServices());

            }
        }

        class ConsoleLogger : ILogger
        {
            public void Log(LogLevel level, string message, object[] args,
                            IFormatProvider formatProvider, string category,
                            string nativeErrorInfo, Exception ex)
            {
                Console.Error.WriteLine(message);
            }

            public bool IsEnabled(LogLevel level)
            {
                // Accept any level.
                return true;
            }
        }

        private class WriteFlowsToCassandra : IComputeAction
        {
            [InstanceResource]
            private readonly IIgnite m_ignite;

            private readonly CassandraWriter m_cassandraWriter;

            public WriteFlowsToCassandra(IPEndPoint endpoint, string keyspace)
            {
                m_cassandraWriter = new CassandraWriter(endpoint, keyspace);
            }

            public void Invoke()
            {
                var sw = new Stopwatch();
                m_cassandraWriter.Initialize();
                var cache = new FlowCache(m_ignite);
                var flows = cache.GetLocalEntries().Select(x => KeyValuePair.Create(x.Key, x.Value));
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Writing {flows.Count()} flows from local cache.");
                sw.Start();
                m_cassandraWriter.WriteFlows(flows);
                sw.Stop();
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Done ({sw.Elapsed}).");
                m_cassandraWriter.Shutdown();
            }
        }
    }
}

