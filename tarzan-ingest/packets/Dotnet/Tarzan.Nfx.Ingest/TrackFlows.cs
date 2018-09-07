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

                    if (fileList.Count == 0)
                    {
                        throw new ArgumentException("At least one source file has to be specified.");
                    }

                    using (var ignite = Ignition.Start(IgniteConfiguration.Default))
                    {
                        IngestDataFromFiles(ignite, fileList.Select(x => x.FullName));

                        Console.WriteLine("Ingestor completed, press CTRL+C (or X) to terminate.");

                        while (true)
                        {
                            var key = Console.ReadKey();
                            if (key.Key == ConsoleKey.X)
                                break;
                        }

                        return 0;
                    }
                });
            };


        static void ExecuteBroadcast(string actionName, ICompute compute, IComputeAction action)
        {

            Console.WriteLine();
            var sw = new Stopwatch();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Starting....");
            sw.Start();
            compute.Broadcast(action);
            sw.Stop();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Done, time elapsed: {sw.Elapsed}.");
        }
        private static void ExecuteRun(string actionName, ICompute compute, IEnumerable<IComputeAction> enumerable)
        {
            Console.WriteLine();
            var sw = new Stopwatch();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Starting....");
            sw.Start();
            compute.Run(enumerable);
            sw.Stop();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Done, time elapsed: {sw.Elapsed}.");
        }

        static void IngestDataFromFiles(IIgnite ignite, IEnumerable<string> fileList)
        {

                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Environment is up.");

                var flowCache = new FlowTable(ignite); 

                var compute = ignite.GetCluster().GetCompute();
                ExecuteRun("Flow Analyzer", compute, fileList.Select(x => new FlowAnalyzer { FileName = x }));

                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Total flows {flowCache.GetCache().GetSize()}");

                ExecuteBroadcast("Service Detector", compute, new ServiceDetector());
                
                ExecuteBroadcast("Dns Analyzer", compute, new DnsAnalyzer());

                ExecuteBroadcast("Http Analyzer", compute, new HttpAnalyzer());

                var stats = new Statistics(ignite);
                var services = stats.GetServices().ToArray();
                var hosts = stats.GetHosts().ToArray();
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
                var cache = new FlowTable(m_ignite).GetCache();
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Writing flows from local cache.");
                sw.Start();
                foreach (var flow in cache.GetLocalEntries())
                {
                    m_cassandraWriter.WriteFlow(flow.Key,flow.Value);
                }
                sw.Stop();
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Done ({sw.Elapsed}).");
                m_cassandraWriter.Shutdown();
            }
        }
    }
}

