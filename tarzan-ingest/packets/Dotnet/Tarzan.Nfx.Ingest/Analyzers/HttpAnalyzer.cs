using Kaitai;
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
            var stream = new KaitaiStream(tcpPacket.PayloadData ?? new byte[0]);
            var httpPacket = new HttpPacket(stream);
            return httpPacket;
        }

        private static List<List<(HttpPacket Packet, PosixTimeval Timeval)>> EmptyAccumulator()
        {
            return new List<List<(HttpPacket, PosixTimeval)>> { new List<(HttpPacket, PosixTimeval)>() };
        }

        private static List<List<(HttpPacket, PosixTimeval)>> Accumulate(List<List<(HttpPacket, PosixTimeval)>> acc, (HttpPacket Packet, PosixTimeval Time) arg)
        {
            // data packets are added to the existing http transaction:
            if (arg.Packet.PacketType == HttpPacketType.Data)
            {
                acc.Last().Add(arg);                
            }
            else
            {
                acc.Add(new List<(HttpPacket, PosixTimeval)> { arg });
            }
            return acc;
        }

        public static IEnumerable<Model.HttpInfo> Inspect(TcpConversation conversation)
        {
            var requests = conversation.RequestFlow.Value.SegmentList.Select(p => (Packet: ParseHttpPacket(p.Packet), Time: p.Timeval)).Aggregate(EmptyAccumulator(), Accumulate);

            var responses = conversation.ResponseFlow.Value.SegmentList.Select(p => (Packet: ParseHttpPacket(p.Packet), Time: p.Timeval)).Aggregate(EmptyAccumulator(), Accumulate);

            var transactions = requests.Zip(responses, (request, response) => (Request:request, Response:response)).Select((item,index) => (Transaction: item, TransactionId: index+1));
                     
            foreach (var (Transaction, TransactionId) in transactions)
            {
                var transactionRequest = Transaction.Request.FirstOrDefault().Packet;
                var transactionResponse = Transaction.Response.FirstOrDefault().Packet;

                if (transactionRequest?.PacketType != HttpPacketType.Request) continue;
                if (transactionResponse?.PacketType != HttpPacketType.Response) continue;

                var (username, password) = ExtractCredentials(transactionRequest.Header.GetLine("Authorization"));
                
                var requestBytes = Transaction.Request.Select(x => x.Packet.Body.Bytes).ToList();
                var responseBytes = Transaction.Response.Select(x => x.Packet.Body.Bytes).ToList();

                var httpInfo = new HttpInfo
                {
                    FlowId = conversation.RequestFlow.Value.FlowId.ToString(),
                    TransactionId = TransactionId.ToString(),
                    Timestamp = new DateTimeOffset(Transaction.Request.FirstOrDefault().Timeval.Date).ToUnixTimeMilliseconds(),
                    Method = transactionRequest.Request.Command,
                    Version = transactionRequest.Request.Version,
                    Uri = transactionRequest.Request.Uri,
                    Host = transactionRequest.Header.Host,
                    UserAgent = transactionRequest.Header.GetLine("User-Agent"),
                    Referrer = transactionRequest.Header.GetLine("Referer", "Referrer"),
                    Username = username,
                    Password = password,
                    RequestHeaders = transactionRequest.Header.Lines.Select(line => $"{line.Name}:{line.Value}").ToList(),
                    ResponseHeaders = transactionResponse.Header.Lines.Select(line => $"{line.Name}:{line.Value}").ToList(),
                    StatusCode = transactionResponse.Response.StatusCode,
                    StatusMessage = transactionResponse.Response.Reason,
                    Client = conversation.RequestFlow.Key.SourceEndpoint.ToString(),
                    Server = conversation.ResponseFlow.Key.SourceEndpoint.ToString(),
                    RequestBodyChunks = requestBytes,
                    ResponseBodyChunks = responseBytes,
                    RequestBodyLength = requestBytes.Sum(p => p.Length),
                    ResponseBodyLength = responseBytes.Sum(p => p.Length),
                    RequestContentType = transactionRequest.Header.GetLine("Content-Type", "ContentType") ?? "application/octet-stream",
                    ResponseContentType = transactionResponse.Header.GetLine("Content-Type", "ContentType") ?? "application/octet-stream",
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
