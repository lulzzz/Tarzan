using Kaitai;
using PacketDotNet;
using PacketDotNet.MiscUtil.Conversion;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using Xunit;

namespace Tarzan.Nfx.PacketDecoders.Tests
{
    public class TlsDecodeTest
    {
        private TlsPacket ParseTlsPacket(TcpPacket tcpPacket)
        {
            try
            {
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
        private TcpPacket ParseTcpPacket(FrameData frame)
        {
            var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
            var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
            return tcpPacket;
        }

        string TcpFlags(TcpPacket packet)
        {
            var flags = new List<string>();
            if(packet.Syn) flags.Add("SYN");
            if(packet.Ack) flags.Add("ACK");
            if(packet.Psh) flags.Add("PSH");
            if(packet.Rst) flags.Add("RST");
            if(packet.Fin) flags.Add("FIN");
            return String.Join(',', flags);
        }
        [Theory]
        [InlineData(@"Resources\ssl2.cap")]
        [InlineData(@"Resources\https\https2-301-get.pcap")]
        public void DecodeSSLCommunication(string filename)
        {
            var frameKeyProvider = new FrameKeyProvider();
            Console.WriteLine($"Test file={filename}:");
            var packets = PacketProvider.LoadPacketsFromResourceFolder(filename).Select(p => new FrameData { Data = p.Data, LinkLayer = (LinkLayerType)p.LinkLayerType, Timestamp = 0 });
            var flows = from packet in packets.Select(p => (Key: frameKeyProvider.GetKey(p), Packet: p))
                        group packet by packet.Key;
            foreach (var flow in flows.Where(x=>IsTlsFlow(x.Key)))
            {
                Console.WriteLine($"{flow.Key}:");
                foreach (var msg in flow)
                {
                    var tcpPacket = ParseTcpPacket(msg.Packet);
                    var tlsPacket = ParseTlsPacket(tcpPacket);
                    bool emptyTcp = (tcpPacket.PayloadData?.Length ?? 0) == 0;

                    var flags = TcpFlags(tcpPacket);
                    var tlsInfo = $"[TLS: Type={tlsPacket?.ContentType.ToString()}]";
                    Console.WriteLine($"  {msg.Key}: {(!emptyTcp ? tlsInfo : "")} [TCP: PayloadSize={tcpPacket?.PayloadData?.Length}, Flags={flags}]");
                }
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

            switch(tlsPacket.Fragment)
            {
                case TlsPacket.TlsHandshake handshake:
                    switch (handshake.Body)
                    {
                        case TlsPacket.TlsCertificate tlscert:
                             var x509cert = new X509Certificate2(tlscert.Certificates.First().Body);
                            break;
                    }
                    break;
                case TlsPacket.TlsClientHello clientHello:
                    foreach(var suite in clientHello.CipherSuites.CipherSuiteList)
                    {
                        Console.WriteLine($"{(TlsCipherSuite)suite}");
                    } 
                    break;
            }
        }
        [Theory]
        [InlineData(@"Resources\ssl\ssl2_client_hello.raw")]
        public void ParseSslRecord(string filename)
        {
            var path = PacketProvider.GetFullPath(filename);
            var bytes = File.ReadAllBytes(path);
            var sslPacket = new SslPacket(new KaitaiStream(bytes));
            switch(sslPacket.Record.Message)
            {
                case SslPacket.SslClientHello clientHello:
                    foreach(var suite in clientHello.CipherSpecs.Entries)
                    {
                        var suiteNumber = (uint)EndianBitConverter.Big.ToUInt16(suite.CipherBytes, 1) + (suite.CipherBytes[0] << 16);
                        var suiteName1 = (TlsCipherSuite)suiteNumber;
                    }
                    break;
            }
        }

    }
}
