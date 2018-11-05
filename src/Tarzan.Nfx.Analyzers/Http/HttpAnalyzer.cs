using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Kaitai;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Packets.Core;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public static class PacketCacheCollection 
    {
        public static IEnumerable<FrameData> GetOrderedPackets(this FrameCacheCollection frameCacheCollection, FlowKey flowKey)
        {
            return frameCacheCollection.GetFrames(flowKey).OrderBy(f=>f.Value.Timestamp).Select(f=>f.Value);
        }
        public static Conversation<IEnumerable<FrameData>> GetConversation(this FrameCacheCollection frameCache, FlowKey flowKey)
        {
            var upflowPackets = frameCache.GetOrderedPackets(flowKey);
            var downflowPackets = frameCache.GetOrderedPackets(flowKey.SwapEndpoints());
            return new Conversation<IEnumerable<FrameData>>(flowKey, upflowPackets, downflowPackets);
        }
    }
    public class HttpAnalyzer : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public string FlowCacheName { get; }

        public IEnumerable<string> FrameCacheNames { get; }

        public string HttpCacheName { get; }

        public HttpAnalyzer(string flowCacheName, IEnumerable<string> frameCacheNames, string httpCacheName)
        {
            FlowCacheName = flowCacheName;
            FrameCacheNames = frameCacheNames;
            HttpCacheName = httpCacheName;
        }

        private HttpPacket ParseHttpPacket(FrameData frame)
        {
            try
            {
                var packet = Packet.ParsePacket((LinkLayers)frame.LinkLayer, frame.Data);
                var tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
                var stream = new KaitaiStream(tcpPacket?.PayloadData ?? new byte[0]);
                var httpPacket = new HttpPacket(stream);
                return httpPacket;
            }
            catch(Exception)
            {
                return new HttpPacket(new KaitaiStream(new byte[0]));
            }
        }

        public IEnumerable<HttpObject> ExtractHttpObjects(Conversation<IEnumerable<FrameData>> conversation)
        {
            var transactions = GetHttpConnections(conversation);

            foreach (var transaction in transactions)
            {
                var theRequest = transaction.Request.FirstOrDefault();
                var theResponse = transaction.Response.FirstOrDefault();

                if (theRequest.Packet?.PacketType != HttpPacketType.Request) continue;
                if (theResponse.Packet?.PacketType != HttpPacketType.Response) continue;

                var (username, password) = ExtractCredentials(theRequest.Packet.Header.GetLine("Authorization"));

                var requestBytes = transaction.Request.Select(x => x.Packet.Body.Bytes).ToList();
                var responseBytes = transaction.Response.Select(x => x.Packet.Body.Bytes).ToList();

                var httpInfo = new HttpObject
                {
                    FlowUid = conversation.ConversationKey.ToString(),
                    ObjectIndex = transaction.Index.ToString("D4"),
                    Timestamp = theRequest.Timeval.ToUnixTimeMilliseconds(),
                    Method = theRequest.Packet.Request.Command,
                    Version = theRequest.Packet.Request.Version,
                    Uri = theRequest.Packet.Request.Uri,
                    Host = theRequest.Packet.Header.Host,
                    UserAgent = theRequest.Packet.Header.GetLine("User-Agent"),
                    Referrer = theRequest.Packet.Header.GetLine("Referer", "Referrer"),
                    Username = username,
                    Password = password,
                    RequestHeaders = theRequest.Packet.Header.Lines.Select(line => $"{line.Name}:{line.Value}").ToList(),
                    ResponseHeaders = theResponse.Packet.Header.Lines.Select(line => $"{line.Name}:{line.Value}").ToList(),
                    StatusCode = theResponse.Packet.Response.StatusCode,
                    StatusMessage = theResponse.Packet.Response.Reason,
                    Client = conversation.ConversationKey.SourceEndpoint.ToString(),
                    Server = conversation.ConversationKey.DestinationEndpoint.ToString(),
                    RequestBodyChunks = requestBytes,
                    ResponseBodyChunks = responseBytes,
                    RequestBodyLength = requestBytes.Sum(p => p.Length),
                    ResponseBodyLength = responseBytes.Sum(p => p.Length),
                    RequestContentType = theRequest.Packet.Header.GetLine("Content-Type", "ContentType") ?? "application/octet-stream",
                    ResponseContentType = theResponse.Packet.Header.GetLine("Content-Type", "ContentType") ?? "application/octet-stream",
                };
                yield return httpInfo;
            }
        }

        private IEnumerable<HttpTransaction> GetHttpConnections(Conversation<IEnumerable<FrameData>> tcpConversation)
        {
            var requests = GetHttpMessages(tcpConversation.Upflow);
            var responses = GetHttpMessages(tcpConversation.Downflow);
            var transactions = requests.Zip(responses, (request, response) => (Request: request, Response: response)).Select((item, index) => new HttpTransaction { Request = item.Request, Response = item.Response, Index = index + 1 });
            return transactions;
        }

        private List<HttpPacketList> GetHttpMessages(IEnumerable<FrameData> tcpFlow)
        {
            List<HttpPacketList> Empty()
            {
                return new List<HttpPacketList> { new HttpPacketList() };
            }
            List<HttpPacketList> Accumulate(List<HttpPacketList> acc, (HttpPacket Packet, PosixTime Time) arg)
            {
                if (arg.Packet.PacketType == HttpPacketType.Data)
                {
                    acc.Last().Add(arg);
                }
                else
                {
                    acc.Add(new HttpPacketList { arg });
                }
                return acc;
            }
            return tcpFlow.Select(f => (Packet: ParseHttpPacket(f), PosixTime.FromUnixTimeMilliseconds(f.Timestamp))).Aggregate(Empty(), Accumulate);
        }

        private (string Username,string Password) ExtractCredentials(string authorization)
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

        void IComputeAction.Invoke()
        {
            var httpCache = CacheFactory.GetOrCreateCache<string, HttpObject>(m_ignite, HttpCacheName);
            var flowCache = CacheFactory.GetOrCreateFlowCache(m_ignite, FlowCacheName);
            var frameCache = new FrameCacheCollection(m_ignite, FrameCacheNames, local:true);

            var httpObjects = flowCache.GetLocalEntries()
                .Where(f => String.Equals(f.Value.ServiceName, "www-http", StringComparison.InvariantCultureIgnoreCase))
                .Where(f => f.Key.SourcePort > f.Key.DestinationPort)
                .Select(f => frameCache.GetConversation(f.Key))
                .SelectMany(c => ExtractHttpObjects(c))
                .Select(x => KeyValuePair.Create(x.ObjectName, x));

            httpCache.PutAll(httpObjects);
        }

        class HttpPacketList : List<(HttpPacket Packet, PosixTime Timeval)>
        {
        }
        struct HttpTransaction 
        {
            public HttpPacketList Request;
            public HttpPacketList Response;
            public int Index;
        }
    }
}
