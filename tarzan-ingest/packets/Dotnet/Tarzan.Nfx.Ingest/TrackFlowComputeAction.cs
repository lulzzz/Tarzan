using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using System;
using System.Diagnostics;
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
            Console.Write($"APP: Start processing file '{FileName}'");

            var device = new FastPcapFileReaderDevice(FileName);
            device.Open();

            var sw = new Stopwatch();
            sw.Start();

            var flowTracker = new FlowTracker(new CaptureDeviceProvider(device));
            flowTracker.CaptureAll();

            device.Close();
            sw.Stop();

            Console.WriteLine($"Done ({sw.Elapsed}), packets={flowTracker.TotalFrameCount}, flows={flowTracker.FlowTable.Count}.");

            var globalFlowTable = new FlowCache(_ignite);
            using (var loader = globalFlowTable.GetDataStreamer())
            {
                // STREAMING: https://apacheignite-net.readme.io/docs/streaming
                // TODO: Currently it does not add flows already in cache, use transformer instead...
                loader.AddData(flowTracker.FlowTable);
            }

            Console.WriteLine($"APP: Processing done.");
        }
    }
}

