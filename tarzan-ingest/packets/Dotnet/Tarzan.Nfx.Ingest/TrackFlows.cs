using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Microsoft.Extensions.CommandLineUtils;
using Netdx.ConversationTracker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest
{
    class TrackFlows
    {
        public static string Name => "track-flows";
        public static Action<CommandLineApplication> Configuration =>
            (CommandLineApplication target) =>
            {
                target.Description = "Track flows and write flow records to the specified output.";

                var optionInputFile = target.Option("-file", "Read packet data from specified PCAP file. The file should have any supported capture file format (including gzipped files).", CommandOptionType.SingleValue);
                var optionInputFolder = target.Option("-folder", "Read packet data from PCAP files in the specified folder.", CommandOptionType.SingleValue);
                var optionInterface = target.Option("-live", "Read packet data from the network interface.", CommandOptionType.SingleValue);
                var optionCassandra = target.Option("-cassandra", "Specifies address of the Cassandra DB node to store flow records.", CommandOptionType.SingleValue);
                var optionKeyspace = target.Option("-namespace", "Specifies the keyspace in Cassandra DB.", CommandOptionType.SingleValue);
                var optionCreate = target.Option("-create", "Creates/initializes the keyspace in Cassandra DB. The existing keyspace will be deleted.", CommandOptionType.NoValue);
                var optionSequential = target.Option("-sequential", "Run in sequential mode.", CommandOptionType.NoValue);

                IEnumerable<(ICaptureDevice Device, FileInfo Info)> GetInputDevice()
                {
                    var devices = new List<(ICaptureDevice Device, FileInfo Info)>();
                    if (optionInputFile.HasValue())
                    {
                        var fileinfo = new FileInfo(optionInputFile.Value());
                        var inputDevice = new CaptureFileReaderDevice(fileinfo.FullName);
                        LinkLayers = inputDevice.LinkType;
                        devices.Add((inputDevice, fileinfo));
                    }
                    if (optionInterface.HasValue())
                    {
                        if (Int32.TryParse(optionInterface.Value(), out var interfaceIndex))
                        {
                            if (interfaceIndex < CaptureDeviceList.Instance.Count)
                            {
                                
                                var inputDevice = CaptureDeviceList.Instance[interfaceIndex];
                                LinkLayers = inputDevice.LinkType;
                                devices.Add((inputDevice, new FileInfo(inputDevice.Name)));
                            }
                            else
                            {
                                throw new ArgumentException($"Interface index: {optionInterface.Value()} is invalid. This value should be between 0 and {CaptureDeviceList.Instance.Count - 1}. Use print-interfaces command to see available options.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid interface index: {optionInterface.Value()}. This should be an integer value between 0 and {CaptureDeviceList.Instance.Count - 1}. Use print-interfaces command to see available options.");
                        }
                    }
                    if(optionInputFolder.HasValue())
                    {
                        var dir = new DirectoryInfo(optionInputFolder.Value());
                        foreach(var fileinfo in dir.EnumerateFiles("*.?cap"))
                        {
                            var inputDevice = new CaptureFileReaderDevice(fileinfo.FullName);
                            LinkLayers = inputDevice.LinkType;
                            devices.Add((inputDevice, fileinfo));
                        }
                    }
                    return devices;
                }


                Action<(ICaptureDevice Device, FileInfo Info)> CreateProcessor(CassandraWriter cassandraWriter)
                {
                    return async ((ICaptureDevice Device, FileInfo Info) input) =>
                    {
                        Console.WriteLine($"{DateTime.Now} Opening input '{input.Device.Name}'.");
                        Console.WriteLine($"{DateTime.Now} Start tracking flows...");
                        var flowTracker = new FlowTracker(input.Device);
                        await flowTracker.TrackAsync();
                        Console.WriteLine($"{DateTime.Now} Tracking flows finished, flow count={flowTracker.Table.Count}");

                        Console.WriteLine($"{DateTime.Now} Start detecting services.");
                        var serviceDetector = new ServiceDetector();
                        foreach (var flow in flowTracker.Table.Entries)
                        {
                            flow.Value.ServiceName = serviceDetector.DetectService(flow.Key, flow.Value);
                        }
                        Console.WriteLine($"{DateTime.Now} Detecting services finished.");

                        Console.WriteLine($"{DateTime.Now} Writing data...");
                        await cassandraWriter.WriteAll(input.Info, flowTracker.Table);
                    };
                }

                target.OnExecute(() =>
                {
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

                    var devices = GetInputDevice().ToArray();

                    Console.WriteLine($"{DateTime.Now} Initializing keyspace '{optionKeyspace.Value()}'...");
                    cassandraWriter.Initialize();

                    var processInput = CreateProcessor(cassandraWriter);

                    if (optionSequential.HasValue())
                    {
                        foreach (var device in devices)
                        {
                            processInput(device);
                        }
                    }
                    else
                    {
                        Parallel.ForEach(devices, processInput);
                    }
                    Console.WriteLine($"{DateTime.Now} All Done. Closing connection to Cassandra..");
                    cassandraWriter.Shutdown();
                    Console.WriteLine($"{DateTime.Now} Ok.");
                    return 0;
                });
            };

        public static LinkLayers LinkLayers { get; private set; }
    }
}

