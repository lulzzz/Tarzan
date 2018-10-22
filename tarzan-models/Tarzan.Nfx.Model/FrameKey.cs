using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Affinity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.PcapLoader
{
    [Serializable]
    public struct FrameKey : IBinarizable
    {
        public int FrameNumber { get; set; }
        [AffinityKeyMapped]
        public int FlowKeyHash { get; set; }

        public void ReadBinary(IBinaryReader reader)
        {
            FrameNumber = reader.ReadInt(nameof(FrameNumber));
            FlowKeyHash = reader.ReadInt(nameof(FlowKeyHash));
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteInt(nameof(FrameNumber), FrameNumber);
            writer.WriteInt(nameof(FlowKeyHash), FlowKeyHash);
        }
    }
}
