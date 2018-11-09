using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Model
{
    [Serializable]
    public class FrameKey : IBinarizable
    {
        public FrameKey()
        {
        }

        public FrameKey(int frameNumber, int flowKeyHash)
        {
            FrameNumber = frameNumber;
            FlowKeyHash = flowKeyHash;
        }

        [QuerySqlField(IsIndexed = true)]
        public int FrameNumber { get; set; }

        [QuerySqlField(IsIndexed = true)]
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

        public override bool Equals(object obj)
        {
            if (!(obj is FrameKey other))
            {
                return false;
            }
            else
            {
                return this.FrameNumber == other.FrameNumber
                    && this.FlowKeyHash == other.FlowKeyHash;
            }
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.CombineHashCodes(FrameNumber.GetHashCode(), FlowKeyHash.GetHashCode());   
        }
    }
}
