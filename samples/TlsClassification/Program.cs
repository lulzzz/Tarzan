using System;
using System.Collections.Generic;
using System.Linq;
using Kaitai;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.Common;
using PacketDotNet;
using PacketDotNet.MiscUtil.Conversion;
using System.IO;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
            }
            if (String.Equals("extract", args[0], StringComparison.InvariantCultureIgnoreCase))
            {
                var frameKeyProvider = new FrameKeyProvider();

                var packets = FastPcapFileReaderDevice.ReadAll(args[1]);
                var flows = from packet in packets.Select((p, i) => (Key: frameKeyProvider.GetKey(p), Value: (Number: i, Packet: p)))
                            group packet by packet.Key;

                foreach (var flow in flows.Where(x => IsTlsFlow(x.Key)))
                {
                    Console.WriteLine($"flow:");
                    Console.WriteLine($"  key: '{flow.Key}'");
                    Console.WriteLine($"  packets:");

                    byte[] getTcpPayload((int Number, TcpPacket Packet) p)
                    {
                        return p.Packet.PayloadData ?? new byte[0];
                    }

                    var tcpStream = new TcpStream<(int Number, TcpPacket Packet)>(getTcpPayload, flow.Select(f => (f.Value.Number,ParseTcpPacket(f.Value.Packet))));
                    var tlsPackets = ParseTlsPacket(new KaitaiStream(tcpStream));
                    foreach (var tlsRecord in tlsPackets)
                    {
                        Console.WriteLine($"TLS={TlsDescription(tlsRecord.Packet)}");
                        foreach(var entry in tcpStream.GetSegments(tlsRecord.Offset, tlsRecord.Length))
                        {
                            var tcp = entry.Packet;
                            var range = entry.Range;
                            Console.WriteLine($"  TCP({tcp.Number}): Length={tcp.Packet.TotalPacketLength}, Payload Length={tcp.Packet.PayloadData?.Length ?? 0}, Flags={TcpFlags(tcp.Packet)}, range={range}");
                        }
                    }
                }
            }
        }


        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ACTION PCAP_FILE");
            Console.WriteLine();
            Console.WriteLine("Actions:");
            Console.WriteLine("  extract - extracts features for TLS flows in form of YAML document.");         
        }
        private static IEnumerable<(long Offset, int Length, TlsPacket Packet)> ParseTlsPacket(KaitaiStream kaitaiStream)
        {
            var packets = new List<(long Offset, int Length, TlsPacket Packet)>();
            try
            {
                while (!kaitaiStream.IsEof)
                {
                    var tlsOffset = kaitaiStream.Pos;
                    var tlsPacket = new TlsPacket(kaitaiStream);
                    var tlsLength = kaitaiStream.Pos - tlsOffset;
                    packets.Add((tlsOffset, (int)tlsLength, tlsPacket));
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            return packets;
        }

        private static bool IsTlsFlow(FlowKey key)
        {
            return key.Protocol == System.Net.Sockets.ProtocolType.Tcp
                && (key.SourcePort == 443 || key.DestinationPort == 443);
        }
        private static TcpPacket ParseTcpPacket(FrameData frame)
        {
            var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
            var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
            return tcpPacket;
        }

        private static string TcpFlags(TcpPacket packet)
        {
            var flags = new List<string>();
            if(packet.Syn) flags.Add("SYN");
            if(packet.Ack) flags.Add("ACK");
            if(packet.Psh) flags.Add("PSH");
            if(packet.Rst) flags.Add("RST");
            if(packet.Fin) flags.Add("FIN");
            return String.Join(',', flags);
        }
        private static string TlsDescription(TlsPacket packet)
        {
            switch(packet.ContentType)
            {
                case TlsPacket.TlsContentType.Handshake:
                    var handshake = packet.Fragment as TlsPacket.TlsHandshake;
                    return $"TLSv1.2 Record Layer: Handshake Protocol: {handshake.MsgType}.";
                case TlsPacket.TlsContentType.ApplicationData:
                    var appdata = packet.Fragment as TlsPacket.TlsApplicationData;
                    return $"TLSv1.2 Record Layer: Application Data, Length={appdata.Body.Length}."; 
                case TlsPacket.TlsContentType.ChangeCipherSpec:
                    return $"TLSv1.2 Record Layer: Change Cipher Spec Protocol.";
                case TlsPacket.TlsContentType.Alert:
                    return $"TLSv1.2 Record Layer: Alert.";
            }
            return $"TLSv1.2 Record Layer.";
        }
    }
}
