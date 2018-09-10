using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using Tarzan.Nfx.Ingest.Flow;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class PacketStreamFactory : ITableConfigurationProvider
    {
        public static PacketStream From(FlowKey flowKey, Frame frame, string flowUid)
        {
            return new PacketStream()
            {
                FlowUid = flowUid,
                FrameList = new List<Frame> { frame }
            };
        }

        public static PacketStream Update(PacketStream packetStream, Frame frame)
        {
            packetStream.FrameList.Add(frame);
            return packetStream;
        }

        public static PacketStream Merge(PacketStream stream1, PacketStream stream2, string flowUid)
        {
            return new PacketStream()
            {
                FlowUid = flowUid,
                FrameList = Enumerable.Concat(stream1.FrameList, stream2.FrameList).ToList()
            };
        }


        public BinaryTypeConfiguration TypeConfiguration =>
            new BinaryTypeConfiguration()
            {
                TypeName = nameof(PacketStream),
                Serializer = new PacketStreamSerializer()
            };

        public CacheConfiguration CacheConfiguration => 
            new CacheConfiguration(nameof(PacketStreamTable))
            {
                CacheMode = CacheMode.Partitioned,
                Backups = 0,
                QueryEntities = new[]
                {
                        new QueryEntity(typeof(PacketStream))
                        {
                            Fields = new []
                            {
                                new QueryField(nameof(PacketStream.FlowUid), typeof(string))
                            }
                        }
                }
         };
    }
}
