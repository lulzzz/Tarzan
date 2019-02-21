using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Datastream;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Model.Observable;
using Xunit;

namespace Tarzan.Nfx.Ignite.Tests
{

    [TestCaseOrderer("Tarzan.Nfx.Ignite.Tests.TestPriorityOrderer", "Tarzan.Nfx.Ignite.Tests")]
    public class Streaming : IClassFixture<IgniteFixture>
    {
        private readonly IgniteFixture m_igniteFixture;

        public Streaming(IgniteFixture igniteFixture)
        {
            m_igniteFixture = igniteFixture;
        }

        [Fact, TestPriority(1)]
        public void StreamWithReceiver()
        {
            var cache = m_igniteFixture.Server.Ignite.GetOrCreateCache<string, string>("Cache");
            using (var dataStreamer = cache.Ignite.GetDataStreamer<string, string>(cache.Name))
            {
                dataStreamer.AllowOverwrite = true;
                dataStreamer.Receiver = new PacketFlowVisitor2();
                    //new PacketFlowVisitor(new MergePacketFlowProcessor());

                for (int i = 0; i < 1000; i++)
                {
                    dataStreamer.AddData(i.ToString(), ""); // new Artifact { Id = i.ToString(), PayloadBin = new byte[i] });
                }

                for (int i = 0; i < 1000; i++)
                {
                    dataStreamer.AddData(i.ToString(), ""); // new Artifact { Id = i.ToString(), PayloadBin = new byte[i] });
                }

                dataStreamer.Flush();
            }
        }
    }

    public sealed class PacketFlowVisitor : IStreamReceiver<string, Artifact>
    {
        private MergePacketFlowProcessor m_updateProcessor;
        public PacketFlowVisitor(MergePacketFlowProcessor updateProcessor)
        {
            this.m_updateProcessor = updateProcessor;
        }
        public void Receive(ICache<string, Artifact> cache, ICollection<ICacheEntry<string, Artifact>> entries)
        {
            foreach (var entry in entries)
            {
                cache.Invoke(entry.Key, m_updateProcessor, entry.Value);
            }
        }
    }
    public class MergePacketFlowProcessor : ICacheEntryProcessor<string, Artifact, Artifact, Artifact>
    {
        public Artifact Process(IMutableCacheEntry<string, Artifact> entry, Artifact arg)
        {
            if (entry.Exists)
            {

                entry.Value = Merge(entry.Value, arg);
            }
            else
            {
                entry.Value = arg;
            }
            return null;
        }

        public static Artifact Merge(Artifact flow1, Artifact flow2)
        {
            if (flow1 == null) throw new ArgumentNullException(nameof(flow1));
            if (flow2 == null) throw new ArgumentNullException(nameof(flow2));

            return new Artifact()
            {
                PayloadBin = ConcatArrays(flow1.PayloadBin, flow2.PayloadBin),
                MimeType = "binary/octet-stream",
            };
        }

        private static byte[] ConcatArrays(byte[] one, byte[] two)
        {
            int length = one.Length + two.Length;
            byte[] sum = new byte[length];
            one.CopyTo(sum, 0);
            two.CopyTo(sum, one.Length);
            return sum;
        }
    }
    public class PacketFlowVisitor2 : IStreamReceiver<string, string>
    {
        public void Receive(ICache<string, string> cache, ICollection<ICacheEntry<string, string>> entries)
        {
            Console.Write('.');
        }
    }
}
