using BenchmarkDotNet.Attributes;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Ingest;
using Tarzan.Nfx.Ingest.Flow;
using Tarzan.Nfx.Model;

namespace PacketDecodersTest
{
    [CoreJob]
    public class GetKeyBenchmark
    {
        [Params(@"Resources\http.cap")]
        public string filename;

        [GlobalSetup]
        public void Setup()
        {
            // read packet from input file:
            _packets = new List<RawCapture>();
            var device = new CaptureFileReaderDevice(filename);
            device.Open();
            RawCapture packet;
            while ((packet = device.GetNextPacket()) != null)
            {
                _packets.Add(packet);
            }
            device.Close();
        }

        List<RawCapture> _packets;

        [Benchmark]
        public void GetKeyFast()
        {
            foreach (var p in _packets)
            {
                var key = FrameKeyProvider.GetKey(p.Data);
            }
        }
        [Benchmark]
        public void GetKeyPacketDotNet()
        {
            foreach (var p in _packets)
            {
                var key = FrameKeyProvider.GetKey(LinkLayerType.Ethernet, p.Data);
            }
        }
    }
}
