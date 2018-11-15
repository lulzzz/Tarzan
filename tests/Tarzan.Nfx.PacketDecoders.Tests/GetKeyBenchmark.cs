using BenchmarkDotNet.Attributes;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Collections.Generic;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;

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
                var key = FrameKeyProvider.GetKeyForEthernetFrame(p.Data);
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
