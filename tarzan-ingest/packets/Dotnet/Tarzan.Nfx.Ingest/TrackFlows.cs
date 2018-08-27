using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Lifecycle;
using Apache.Ignite.Core.Log;
using Apache.Ignite.Core.Resource;
using Microsoft.Extensions.CommandLineUtils;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    partial class TrackFlows
    {
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

                var optionRunInParallel = target.Option("-parallel", "Run in parallel mode.", CommandOptionType.NoValue);
                var optionIgnite = target.Option("-ignite", "Run as Ignite client. Processing occurs in Ignite server cluster.", CommandOptionType.NoValue);

                IEnumerable<(ICaptureDevice Device, FileInfo Info)> GetDevicesForFiles(IEnumerable<FileInfo> inputFileList)
                {
                    var devices = new List<(ICaptureDevice Device, FileInfo Info)>();
                    foreach(var fileinfo in inputFileList)
                    { 
                        var inputDevice = new CaptureFileReaderDevice(fileinfo.FullName);
                        devices.Add((inputDevice, fileinfo));
                    }
                    return devices;
                }

                Action<(ICaptureDevice Device, FileInfo Info)> CreateProcessor(CassandraWriter cassandraWriter)
                {
                    return async ((ICaptureDevice Device, FileInfo Info) input) =>
                    {
                        Console.WriteLine($"{DateTime.Now} Opening input '{input.Device.Name}'.");
                        Console.WriteLine($"{DateTime.Now} Start tracking flows...");
                        input.Device.Open();
                        var flowTracker = new FlowTracker(new CaptureDeviceProvider(input.Device));
                        flowTracker.CaptureAll();
                        input.Device.Close();

                        Console.WriteLine($"{DateTime.Now} Tracking flows finished, flow count={flowTracker.FlowTable.Count}");

                        Console.WriteLine($"{DateTime.Now} Start detecting services.");

                        Console.WriteLine($"{DateTime.Now} Detecting services finished.");

                        Console.WriteLine($"{DateTime.Now} Writing data...");
                        await cassandraWriter.WriteAll(input.Info, flowTracker.FlowTable);
                    };
                }


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

                    // initialize Cassandra datastore:
                    if (!optionKeyspace.HasValue())
                    {
                        throw new ArgumentException("Keyspace is not specified.");
                    }

                    var cassandraWriter = new CassandraWriter(IPEndPoint.Parse(optionCassandra.Value() ?? "localhost:9042", 9042), optionKeyspace.Value());

                    if (optionCreate.HasValue())
                    {
                        Console.WriteLine($"{DateTime.Now} Deleting keyspace '{optionKeyspace.Value()}'...");
                        cassandraWriter.DeleteKeyspace();
                    }

                    Console.WriteLine($"{DateTime.Now} Initializing keyspace '{optionKeyspace.Value()}'...");
                    cassandraWriter.Initialize();

                    // run processing pipeline:

                    if (optionIgnite.HasValue())
                    {
                        RunAsIgnite(cassandraWriter, fileList.Select(x => x.FullName));
                        return 0;
                    }
                    else
                    {
                        RunInProcess(fileList.Select(x => x.FullName), optionRunInParallel.HasValue());
                    }

                    Console.WriteLine($"{DateTime.Now} All Done. Closing connection to Cassandra..");
                    cassandraWriter.Shutdown();
                    Console.WriteLine($"{DateTime.Now} Ok.");
                    return 0;
                });
            };


        static void RunInProcess(IEnumerable<string> fileList, bool parallel)
        {
            if (parallel)
            {
                Parallel.ForEach(fileList, x => { var c = new TrackFlowComputeAction { FileName = x }; c.Invoke(); });
            }
            else
            {
                foreach (var filename in fileList)
                {
                    var c = new TrackFlowComputeAction { FileName = filename }; c.Invoke();
                }
            }
        }

        static void RunAsIgnite(CassandraWriter cassandraWriter, IEnumerable<string> fileList)
        {          
            using (var ignite = Ignition.Start(GlobalIgniteConfiguration.Default))
            {
                var compute = ignite.GetCluster().GetCompute();
                compute.Run(fileList.Select(x=> new TrackFlowComputeAction { FileName = x }));

                var cache = FlowCache.GetCache(ignite);
                Console.WriteLine($"Total flows {cache.Count()}");

                var serviceDetector = new ServiceDetector();
                foreach (var flow in cache)
                {
                    flow.Value.ServiceName = serviceDetector.DetectService(flow.Key, flow.Value);
                    cache.Invoke()
                    cache.Put(flow.Key, flow.Value);
                }

                cassandraWriter.WriteAll(input.Info, flowTracker.FlowTable);

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

        class UpdateServiceName : ICacheEntryProcessor<PacketFlowKey, PacketStream, string, PacketStream>
        {
            public PacketStream Process(IMutableCacheEntry<PacketFlowKey, PacketStream> entry, string arg)
            {
                entry.Value.ServiceName = arg;
                return entry.Value;
            }
        }

        private class DetectServiceComputeAction : IComputeAction
        {
            ServiceDetector m_serviceDetector = new ServiceDetector();
            [InstanceResource]
            private readonly IIgnite m_ignite;
            public void Invoke()
            {
                // GET all local flows and identify their service names:
                var cache = FlowCache.GetLocalCache(m_ignite);
                var updateServiceName = new UpdateServiceName();
                foreach (var flow in cache)
                {
                    var serviceName = m_serviceDetector.DetectService(flow.Key, flow.Value);
                    cache.Invoke(flow.Key, updateServiceName, serviceName);
                }
            }
        }
    }
}

