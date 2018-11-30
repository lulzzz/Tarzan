using System.Collections.Generic;
using System.Linq;
using Tarzan.Nfx.Model;
using PacketDotNet;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    class TcpStreamConversation : Conversation<TcpStream<(PacketMeta Meta, TcpPacket Packet)>>
    {
        public TcpStreamConversation(FlowKey flowKey, TcpStream<(PacketMeta Meta, TcpPacket Packet)> upflow, TcpStream<(PacketMeta Meta, TcpPacket Packet)> downflow) : base(flowKey, upflow, downflow)
        {
        }

        public static IEnumerable<TcpStreamConversation> CreateConversations(IDictionary<FlowKey, IEnumerable<(int Number, FrameData Packet)>> flowDictionary)
        {
            foreach (var key in flowDictionary.Keys.Where(key => key.SourcePort > key.DestinationPort))
            {
                var upflow = MakeTcpStreamFromFrames(flowDictionary[key].OrderBy(f => f.Number));
                var downflow = MakeTcpStreamFromFrames(flowDictionary[key.SwapEndpoints()].OrderBy(f => f.Number));
                yield return new TcpStreamConversation(key, upflow, downflow);
            }
        }

        private static TcpPacket ParseTcpPacket(FrameData frame)
        {
            var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
            var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
            return tcpPacket;
        }

        private static byte[] GetTcpPayload((PacketMeta NumberOffset, TcpPacket Packet) p)
        {
            return p.Packet.PayloadData ?? new byte[0];
        }

        private static TcpStream<(PacketMeta Meta, TcpPacket Packet)> MakeTcpStreamFromFrames(IEnumerable<(int Number, FrameData Frame)> frames)
        {
            var packets = frames.Select(f => (new PacketMeta { Number = f.Number, Timestamp = f.Frame.Timestamp }, ParseTcpPacket(f.Frame)));
            var tcpStream = new TcpStream<(PacketMeta NumberOffset, TcpPacket Packet)>(GetTcpPayload, packets);
            return tcpStream;
        }
    }
}
