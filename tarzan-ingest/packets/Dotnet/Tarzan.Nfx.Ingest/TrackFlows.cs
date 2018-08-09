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

                var inputFile = target.Option("-r", "Read packet data from infile, can be any supported capture file format (including gzipped files).", CommandOptionType.SingleValue);
                var captureInterface = target.Option("-i", "Set the name of the network interface or pipe to use for live packet capture.", CommandOptionType.SingleValue);
                var cassandraNode = target.Option("-w", "Specifies address of the Cassandra DB node to store flow records.", CommandOptionType.SingleValue);
                var cassandraKeyspace = target.Option("-n", "Specifies the namespace/keyspace in Cassandra DB.", CommandOptionType.SingleValue);

                ICaptureDevice GetInputDevice()
                {
                    if (!inputFile.HasValue() && !captureInterface.HasValue())
                    {
                        throw new ArgumentException("Either input file (-r <infile>) or capture interface (-i <capint>) must be specified.");
                    }
                    ICaptureDevice inputDevice = null;
                    if (inputFile.HasValue())
                    {
                        inputDevice = new SharpPcap.LibPcap.CaptureFileReaderDevice(inputFile.Value());
                        linkLayers = inputDevice.LinkType;
                    }
                    if (captureInterface.HasValue())
                    {
                        if (Int32.TryParse(captureInterface.Value(), out var interfaceIndex))
                        {
                            if (interfaceIndex < CaptureDeviceList.Instance.Count)
                            {
                                inputDevice = CaptureDeviceList.Instance[interfaceIndex];
                                linkLayers = inputDevice.LinkType;
                            }
                            else
                            {
                                throw new ArgumentException($"Interface index: {captureInterface.Value()} is invalid. This value should be between 0 and {CaptureDeviceList.Instance.Count - 1}. Use print-interfaces command to see available options.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid interface index: {captureInterface.Value()}. This should be an integer value between 0 and {CaptureDeviceList.Instance.Count - 1}. Use print-interfaces command to see available options.");
                        }
                    }
                    return inputDevice;
                }

                target.OnExecute(() =>
                {                    
                    var device = GetInputDevice();



                    var flowTracker = new FlowTracker(device);
                    flowTracker.Track();

                    var cassandraWriter = new CassandraWriter(IPEndPoint.Parse(cassandraNode.Value() ?? "localhost:9042", 9042), cassandraKeyspace.Value() ?? $"ingest_{DateTime.Now.ToString()}");

                    cassandraWriter.DeleteKeyspace();

                    cassandraWriter.Setup();

                    cassandraWriter.Write(flowTracker.Table);

                    cassandraWriter.Shutdown();                   
                    return 0;
                });
            };

        public static LinkLayers linkLayers { get; private set; }
    }
}

