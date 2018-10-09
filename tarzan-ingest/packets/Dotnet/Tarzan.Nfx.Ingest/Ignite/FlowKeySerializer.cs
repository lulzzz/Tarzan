using Apache.Ignite.Core.Binary;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.FlowTracker;

namespace Tarzan.Nfx.Ingest.Ignite
{
    class FlowKeySerializer : IBinarySerializer
    {
        public void ReadBinary(object obj, IBinaryReader reader)
        {
            var flowKey = (FlowKey)obj;
            var bytes = reader.ReadByteArray(nameof(FlowKey.Bytes));
            if (bytes == null || bytes.Length != 40)
            {
                throw new ArgumentOutOfRangeException($"Invalid size of {nameof(FlowKey.Bytes)}. Must be exactly 40 bytes.");
            }
            flowKey.Reload(bytes);
        }

        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            var flowKey = (FlowKey)obj;
            writer.WriteByteArray(nameof(FlowKey.Bytes), flowKey.Bytes);
            writer.WriteInt(nameof(FlowKey.HashCode), flowKey.HashCode);
        }
    }
}
