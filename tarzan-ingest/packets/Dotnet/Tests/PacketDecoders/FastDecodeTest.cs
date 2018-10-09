using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Netdx.PacketDecoders;
using Netdx.Packets.Base;
using SharpPcap;
using SharpPcap.LibPcap;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Ingest;
using Tarzan.Nfx.Ingest.Flow;
using Xunit;

namespace PacketDecodersTests
{

    public class FastDecodeTest
    {
        static string GetFullPath(string resourcePath)
        {
            var location = typeof(FastDecodeTest).GetTypeInfo().Assembly.Location;
            var dirPath = Path.GetDirectoryName(location);
            return Path.Combine(dirPath, resourcePath);
        }


        [Theory]
        [InlineData(@"Resources\http.cap")]
        public void LoadAndParsePacket(string filename)
        {
            var packets = LoadPackets(GetFullPath(filename));
            var flows = from packet in packets.Select(p => (Key: FrameKeyProvider.GetKey(p.Data), Packet: p))
                        group packet by packet.Key;
            var result = flows.ToList();
        }

      
        private IList<RawCapture> LoadPackets(string filename)
        {
            var packetList = new List<RawCapture>();
            var device = new CaptureFileReaderDevice(filename);
            device.Open();
            RawCapture packet;
            while ((packet = device.GetNextPacket()) != null)
            {
                packetList.Add(packet);
            }
            device.Close();
            return packetList;
        }



       
    }
}
