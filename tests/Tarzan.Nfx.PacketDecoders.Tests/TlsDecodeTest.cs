using Kaitai;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Packets.Common;
using Xunit;

namespace Tarzan.Nfx.PacketDecoders.Tests
{
    public class TlsDecodeTest
    {
        private TlsRecord ParseTlsPacket(RawCapture frame)
        {
            try
            {
                var packet = Packet.ParsePacket(frame.LinkLayerType, frame.Data);
                var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
                var stream = new KaitaiStream(tcpPacket?.PayloadData ?? new byte[0]);
                var httpPacket = new TlsRecord(stream);
                return httpPacket;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [Theory]
        [InlineData(@"Resources\http.cap")]
        public void LoadAndParsePacket(string filename)
        {
            var packets = PacketProvider.LoadPacketsFromResourceFolder(filename);
            var flows = from packet in packets.Select(p => (Key: FrameKeyProvider.GetKey(p.Data), Packet: p))
                        group packet by packet.Key;
            foreach (var flow in flows)
            {
                var tlsFLow = flow.Select(x => (x.Key, ParseTlsPacket(x.Packet)));
                foreach (var msg in tlsFLow)
                    Console.WriteLine($"{msg.Key}: {msg.Item2}");
            }
        }
    }
}
