using Apache.Ignite.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tarzan.Nfx.IgniteServer;
using Tarzan.Nfx.Model;
using Xunit;

namespace Tarzan.Nfx.Ignite.Tests
{



    [TestCaseOrderer("Tarzan.Nfx.Ignite.Tests.TestPriorityOrderer", "Tarzan.Nfx.Ignite.Tests")]
    public class QueryableCache : IClassFixture<IgniteFixture>
    {
        const int FRAMES_COUNT = 1000;
        private readonly IgniteFixture m_igniteFixture;

        public QueryableCache(IgniteFixture igniteFixture)
        {
            m_igniteFixture = igniteFixture;
        }

        [Fact, TestPriority(1)]
        public void FrameCachePut()
        {
            var frames = CacheFactory.GetOrCreateFrameCache(m_igniteFixture.Server.Ignite, "frames");
            frames.Clear();
            for (var i = 1; i <= FRAMES_COUNT; i++)
            {
                frames.Put(new FrameKey { FrameNumber = i, FlowKeyHash = 5678 }, new FrameData { Data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, LinkLayer = LinkLayerType.Ethernet, Timestamp = 123456789 + 10 * i });
            }
            Assert.Equal(FRAMES_COUNT, frames.GetSize());
        }

        [Fact, TestPriority(2)]
        public void FrameCacheGet()
        {
            var frames = CacheFactory.GetOrCreateFrameCache(m_igniteFixture.Server.Ignite, "frames");
            var frameData = frames.Get(new FrameKey { FrameNumber = 100, FlowKeyHash = 5678 });
            Assert.Equal(new FrameData { Data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, LinkLayer = LinkLayerType.Ethernet, Timestamp = 123456789 + 1000 }, frameData);
        }
        [Fact, TestPriority(2)]
        public void FrameCacheWhereFlowHash()
        {
            var frames = CacheFactory.GetOrCreateFrameCache(m_igniteFixture.Server.Ignite, "frames");
            var allFramesFor5678 = frames.AsCacheQueryable().Where(x => x.Key.FlowKeyHash == 5678).ToList();
            Assert.Equal(FRAMES_COUNT, allFramesFor5678.Count);
        }
        [Fact, TestPriority(2)]
        public void FrameCacheWhereRange()
        {
            var frames = CacheFactory.GetOrCreateFrameCache(m_igniteFixture.Server.Ignite, "frames");
            var frames100to200 = frames.AsCacheQueryable().Where(x => x.Key.FrameNumber >= 100 && x.Key.FrameNumber < 200).ToList();
            Assert.Equal(100, frames100to200.Count);
            Assert.All(frames100to200, x => Assert.True(x.Key.FrameNumber >= 100 && x.Key.FrameNumber < 200));
        }

        [Fact, TestPriority(1)]
        public void FlowCachePut()
        {
            var flows = CacheFactory.GetOrCreateFlowCache(m_igniteFixture.Server.Ignite, "flows");
            flows.Put(FlowKey.Create(ProtocolType.Tcp, IPAddress.Parse("192.168.1.1"), 35346, IPAddress.Parse("147.229.11.100"), 80),
                    new FlowData
                    {
                        Protocol = "TCP",
                        SourceAddress = "192.168.1.1",
                        SourcePort = 35346,
                        DestinationAddress = "147.229.11.100",
                        DestinationPort = 80,
                        FirstSeen = 123456789,
                        LastSeen = 123456798,
                        Octets = 88888,
                        Packets = 11,
                        ServiceName = "http-www"
                    }
                );
        }
        [Fact, TestPriority(2)]
        public void FlowCacheGet()
        {
            var flows = CacheFactory.GetOrCreateFlowCache(m_igniteFixture.Server.Ignite, "flows");
            var flowData = flows.Get(FlowKey.Create(ProtocolType.Tcp, IPAddress.Parse("192.168.1.1"), 35346, IPAddress.Parse("147.229.11.100"), 80));
        }
        [Fact, TestPriority(2)]
        public void FlowCacheWhere()
        {
            var flows = CacheFactory.GetOrCreateFlowCache(m_igniteFixture.Server.Ignite, "flows");
            var flow = flows.AsCacheQueryable().Where(x => x.Value.ServiceName == "http-www").ToList();
            Assert.NotEmpty(flow);
        }

        public void Dispose()
        {

        }
    }
}
