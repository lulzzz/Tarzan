using Kaitai;
using Netdx.ConversationTracker;
using Netdx.Packets.Core;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public static class HttpAnalyzer
    {
        private static HttpPacket ParseHttpPacket(Packet packet)
        {
            var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
            var stream = new KaitaiStream(tcpPacket.PayloadData);
            var httpPacket = new Netdx.Packets.Core.HttpPacket(stream);
            return httpPacket;
        }

        public static IEnumerable<Model.HttpInfo> Inspect(KeyValuePair<FlowKey, FlowRecordWithPackets> requestFlow, KeyValuePair<FlowKey,FlowRecordWithPackets> responseFlow)
        {
            var requests = requestFlow.Value.PacketList.Select(p => (Packet: ParseHttpPacket(p.packet), Time: p.time)).Where(p => p.Packet.Request != null);
            var responses = responseFlow.Value.PacketList.Select(p => (Packet: ParseHttpPacket(p.packet), Time: p.time)).Where(p => p.Packet.Response != null);

            var transactions = requests.Zip(responses, (request, response) => (Request:request, Response:response)).Select((item,index) => (Transaction: item, TransactionId: index+1));

            foreach (var (Transaction, TransactionId) in transactions)
            {
                var (username, password) = ExtractCredentials(Transaction.Request.Packet.Header.GetLine("Authorization"));

                var httpInfo = new HttpInfo
                {
                    RequestFlowId = requestFlow.Value.FlowId.ToString(),
                    ResponseFlowId = responseFlow.Value.FlowId.ToString(),
                    TransactionId = TransactionId.ToString(),
                    Timestamp = new DateTimeOffset(Transaction.Request.Time.Date).ToUnixTimeMilliseconds(),
                    Method = Transaction.Request.Packet.Request.Command,
                    Version = Transaction.Request.Packet.Request.Version,
                    Uri = Transaction.Request.Packet.Request.Uri,
                    Host = Transaction.Request.Packet.Header.Host,
                    UserAgent = Transaction.Request.Packet.Header.GetLine("User-Agent"),
                    Referrer = Transaction.Request.Packet.Header.GetLine("Referer", "Referrer"),
                    Username = username,
                    Password = password,
                    RequestHeaders = Transaction.Request.Packet.Header.Lines.Select(line => $"{line.Name}:{line.Value}").ToList(),
                    ResponseHeaders = Transaction.Response.Packet.Header.Lines.Select(line => $"{line.Name}:{line.Value}").ToList(),                    
                    StatusCode = Transaction.Response.Packet.Response.StatusCode,
                    StatusMessage = Transaction.Response.Packet.Response.Reason,
                    Client = requestFlow.Key.SourceEndpoint.ToString(),
                    Server = responseFlow.Key.SourceEndpoint.ToString()
                };
                yield return httpInfo;
            }
        }

        private static (string Username,string Password) ExtractCredentials(string authorization)
        {
            if (authorization == null) return (null, null);

            var authorizationParts = authorization.Split(' ');
            if (authorizationParts.Count() != 2) return (null, null);

            var method = authorizationParts[0];
            var credentials = authorizationParts[1];
            if (method.Equals("base",StringComparison.InvariantCultureIgnoreCase))
            {
                var credentialsBytes = Convert.FromBase64String(credentials);                
                var userPasswd = ASCIIEncoding.ASCII.GetString(credentialsBytes).Split(':');
                return (userPasswd.ElementAtOrDefault(0), userPasswd.ElementAtOrDefault(1));
            }
            return (null, null);
        }
    }
}
