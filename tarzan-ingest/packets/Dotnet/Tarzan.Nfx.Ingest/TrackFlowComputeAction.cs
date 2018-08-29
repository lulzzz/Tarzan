using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Datastream;
using Apache.Ignite.Core.Resource;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    class TrackFlowComputeAction : IComputeAction
    {
        public string FileName { get; set; }

        [InstanceResource]
        private readonly IIgnite _ignite;

        public void Invoke()
        {
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Start processing file '{FileName}'...");

            var device = new FastPcapFileReaderDevice(FileName);
            device.Open();

            var sw = new Stopwatch();
            sw.Start();

            var flowTracker = new FlowTracker(new CaptureDeviceProvider(device));
            flowTracker.CaptureAll();

            device.Close();
           
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Done ({sw.Elapsed}), packets={flowTracker.TotalFrameCount}, flows={flowTracker.FlowTable.Count}.");

            sw.Restart();

            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Streaming to global FLOW CACHE...");


            // use (parallel) update operation for adding data into the global cache instead of Stream Loader (see why bellow)
            var globalFlowTable = new FlowCache(_ignite);
            Parallel.ForEach(flowTracker.FlowTable, flow =>
                {
                    globalFlowTable.Update(flow.Key, flow.Value);
                });

            /* STREAM LOADER CODE:
             * Find out how to correclty use stream transformer to insert/update data in the global cache.
             * The probem is that the arg parameter of Transform is always null.
             * Also .net core causes exception because serialization of delegates is not supported.
            using (var loader = globalFlowTable.GetDataStreamer())
            {
                // STREAMING: https://apacheignite-net.readme.io/docs/streaming
                loader.AllowOverwrite = true;
                loader.Receiver = new StreamTransformer<PacketFlowKey, PacketStream, PacketStream, PacketStream>(new FlowCache.MergePacketStream(globalFlowTable.Cache));
                loader.AddData(flowTracker.FlowTable);
            } */

            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] INGEST: Done ({sw.Elapsed}).");
            sw.Stop();
        }
    }
}

