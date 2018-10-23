using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Affinity;
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
        public int FlowKeyHash { get; set; }

        public void ReadBinary(IBinaryReader reader)
        {
#if !DEBUG
            var rawReader = reader.GetRawReader();
            FrameNumber = rawReader.ReadInt();
            FlowKeyHash = rawReader.ReadInt();
#else
            FrameNumber = reader.ReadInt(nameof(FrameNumber));
            FlowKeyHash = reader.ReadInt(nameof(FlowKeyHash));
#endif
        }

        public void WriteBinary(IBinaryWriter writer)
        {
#if !DEBUG
            var rawWriter = reader.GetRawWriter();
            rawWriter.WriteInt(FrameNumber);
            rawWriter.WriteInt(FlowKeyHash);
#else
            writer.WriteInt(nameof(FrameNumber), FrameNumber);
            writer.WriteInt(nameof(FlowKeyHash), FlowKeyHash);
#endif
        }
    }
}
