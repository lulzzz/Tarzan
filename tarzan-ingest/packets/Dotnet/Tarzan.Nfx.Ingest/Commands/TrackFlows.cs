using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Log;
using Apache.Ignite.Core.Resource;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tarzan.Nfx.Ingest.Analyzers;
using Tarzan.Nfx.Ingest.Ignite;
using Tarzan.Nfx.Ingest.Utils;

namespace Tarzan.Nfx.Ingest
{
    public class TrackFlows : IApplicationCommand
    {
        private readonly IServiceProvider m_serviceProvider;

        public TrackFlows(IServiceProvider serviceProvider)
        {
            m_serviceProvider = serviceProvider;
        }

        public string Name => "track-flows";
        public void ExecuteCommand(CommandLineApplication target)
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

                return Process(fileList);
            });
        }

        public int Process(IList<FileInfo> fileList)
        {
            var igniteConfiguration = m_serviceProvider.GetService<IgniteConfiguration>();
            using (var ignite = Ignition.Start(igniteConfiguration))
            {
                var sw = new Stopwatch();
                sw.Start();
                IngestDataFromFiles(ignite, fileList.Select(x => x.FullName));
                sw.Stop();
                Console.WriteLine($"TOTAL Running time {sw.Elapsed}");
                Console.WriteLine("Ingestor completed, press CTRL+C (or X) to terminate.");

                while (true)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.X)
                        break;
                }

                return 0;
            }
        }

        void ExecuteBroadcast(string actionName, ICompute compute, IComputeAction action)
        {

            Console.WriteLine();
            var sw = new Stopwatch();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Starting....");
            sw.Start();
            compute.Broadcast(action);
            sw.Stop();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Done, time elapsed: {sw.Elapsed}.");
        }
        static void ExecuteRun(string actionName, ICompute compute, IEnumerable<IComputeAction> enumerable)
        {
            Console.WriteLine();
            var sw = new Stopwatch();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Starting....");
            sw.Start();
            compute.Run(enumerable);
            sw.Stop();
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: {actionName}: Done, time elapsed: {sw.Elapsed}.");
        }

        void IngestDataFromFiles(IIgnite ignite, IEnumerable<string> fileList)
        {

            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Environment is up.");

            var flowCache = new PacketFlowTable(ignite);

            var compute = ignite.GetCluster().GetCompute();
            ExecuteRun("Flow Analyzer", compute, fileList.Select(x => new FlowAnalyzer { FileName = x }));

            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Total flows {flowCache.GetCache().GetSize()}");

            return;

            ExecuteBroadcast("Service Detector", compute, new ServiceDetector());

            ExecuteBroadcast("Dns Analyzer", compute, new DnsAnalyzer());

            ExecuteBroadcast("Http Analyzer", compute, new HttpAnalyzer());

            var stats = new Statistics(ignite);
            var services = stats.GetServices().ToArray();
            var hosts = stats.GetHosts().ToArray();
        }
    }
}

