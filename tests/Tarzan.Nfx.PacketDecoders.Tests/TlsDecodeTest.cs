using Kaitai;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using Xunit;

namespace Tarzan.Nfx.PacketDecoders.Tests
{
    public class TlsDecodeTest
    {
        private TlsPacket ParseTlsPacket(RawCapture frame)
        {
            try
            {
                var packet = Packet.ParsePacket(frame.LinkLayerType, frame.Data);
                var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
                if (tcpPacket?.PayloadData?.Length > 0)
                {
                    return new TlsPacket(new KaitaiStream(tcpPacket.PayloadData));
                }
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [Theory]
        [InlineData(@"Resources\ssl2.cap")]
        public void LoadAndParsePacket(string filename)
        {
            var packets = PacketProvider.LoadPacketsFromResourceFolder(filename);
            var flows = from packet in packets.Select(p => (Key: FrameKeyProvider.GetKey(p.Data), Packet: p))
                        group packet by packet.Key;
            foreach (var flow in flows.Where(x=>IsTlsFlow(x.Key)))
            {
                var tlsFLow = flow.Select(x => (x.Key, ParseTlsPacket(x.Packet)));
                foreach (var msg in tlsFLow)
                    Console.WriteLine($"{msg.Key}: {msg.Item2}");
            }
        }

        private bool IsTlsFlow(FlowKey key)
        {
            return key.Protocol == System.Net.Sockets.ProtocolType.Tcp
                && (key.SourcePort == 443 || key.DestinationPort == 443);
        }

        [Theory]
        [InlineData(@"Resources\ssl\ssl3_application_data.raw")]
        [InlineData(@"Resources\ssl\ssl3_handshake_encrypted_handshake_message.raw")]
        [InlineData(@"Resources\ssl\ssl3_change_cipher_spec.raw")]
        [InlineData(@"Resources\ssl\ssl3_handskahe_client_key_exchange.raw")]
        [InlineData(@"Resources\ssl\ssl3_handshake_server_hello_done.raw")]
        [InlineData(@"Resources\ssl\ssl3_handshake_certificate.raw")]
        [InlineData(@"Resources\ssl\ssl3_server_hello.raw")]
        public void ParseTlsRecord(string filename)
        {
            var path = PacketProvider.GetFullPath(filename);
            var bytes = File.ReadAllBytes(path);
            var tlsPacket = new TlsPacket(new KaitaiStream(bytes));
        }
        [Theory]
        [InlineData(@"Resources\ssl\ssl2_client_hello.raw")]
        public void ParseSslRecord(string filename)
        {
            var path = PacketProvider.GetFullPath(filename);
            var bytes = File.ReadAllBytes(path);
            var tlsPacket = new SslPacket(new KaitaiStream(bytes));
        }
    }
}
