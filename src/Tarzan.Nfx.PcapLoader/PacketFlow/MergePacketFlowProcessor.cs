using Apache.Ignite.Core.Cache;
using System;
using Tarzan.Nfx.Model.Observable;

namespace Tarzan.Nfx.PcapLoader.PacketFlow
{
    [Serializable]
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
}
