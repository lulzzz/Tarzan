using Kaitai;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Packets.Core;
using Xunit;

namespace Tarzan.Nfx.PacketDecoders.Tests
{
    public class HttpDecodeTest
    {
        private HttpPacket ParseHttpPacket(RawCapture frame)
        {
            try
            {
                var packet = Packet.ParsePacket(frame.LinkLayerType, frame.Data);
                var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
                var stream = new KaitaiStream(tcpPacket?.PayloadData ?? new byte[0]);
                var httpPacket = new HttpPacket(stream);
                return httpPacket;
            }
            catch (Exception)
            {
                return new HttpPacket(new KaitaiStream(new byte[0]));
            }
        }

        [Theory]
        [InlineData(@"Resources\http.cap")]
        public void LoadAndParsePacket(string filename)
        {
            var packets = PacketProvider.LoadPacketsFromResourceFolder(filename);
            var flows = from packet in packets.Select(p => (Key: FrameKeyProvider.GetKey(p.Data), Packet: p))
                        group packet by packet.Key;
            foreach(var flow in flows)
            {
                var httpFlow = flow.Select(x => (x.Key, ParseHttpPacket(x.Packet)));
                foreach(var msg in httpFlow)
                    Console.WriteLine($"{msg.Key}: {msg.Item2}");
            }
        }
    }
}
