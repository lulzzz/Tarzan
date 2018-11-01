using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    [Serializable]
    public struct FrameKey : IBinarizable
    {
        public int FrameNumber { get; set; }
        [AffinityKeyMapped]
        [QuerySqlField(IsIndexed=true)]
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
