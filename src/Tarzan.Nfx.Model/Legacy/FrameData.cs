using Apache.Ignite.Core.Binary;
using System;

namespace Tarzan.Nfx.Model
{
    [Serializable]
    public class FrameData : IBinarizable
    {
        public long Timestamp { get; set; }

        public LinkLayerType LinkLayer { get; set; }

        public byte[] Data { get; set; }

        /// <summary>
        /// Serializes <see cref="FrameData"/> to byte array. It has the following format: | LinkLayer(4) | TimeStamp (8) | DataLen (4) | Data (DataLen) |.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            var buffer = new byte[Data.Length + sizeof(long) + sizeof(int) + sizeof(int)];
            BitConverter.GetBytes((int)LinkLayer).CopyTo(buffer, 0);
            BitConverter.GetBytes(Timestamp).CopyTo(buffer, sizeof(int));
            BitConverter.GetBytes(Data.Length).CopyTo(buffer, sizeof(long) + sizeof(int));
            Data.CopyTo(buffer, sizeof(long) + sizeof(int) + sizeof(int));
            return buffer;
        }

        /// <summary>
        /// Reads the <see cref="FrameData"/> object from the provided byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static FrameData FromBytes(byte[] array, int offset)
        {
            var bytes = new byte[BitConverter.ToInt32(array, sizeof(long) + sizeof(int))];
            Buffer.BlockCopy(array, offset + sizeof(long) + sizeof(int) + sizeof(int), bytes, 0, bytes.Length);
            return new FrameData
            {
                Timestamp = BitConverter.ToInt64(array, offset),
                LinkLayer = (LinkLayerType)BitConverter.ToInt32(array, offset + sizeof(long)),
                Data = bytes
            };
        }

        public void ReadBinary(IBinaryReader reader)
        {
            this.Timestamp = reader.ReadLong(nameof(Timestamp));
            this.LinkLayer = (LinkLayerType)reader.ReadInt(nameof(LinkLayer));
            this.Data = reader.ReadByteArray(nameof(Data));
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteLong(nameof(Timestamp), Timestamp);
            writer.WriteInt(nameof(LinkLayer), (int)LinkLayer);
            writer.WriteByteArray(nameof(Data), Data); 
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FrameData other)) return false;
            return this.LinkLayer == other.LinkLayer
                && this.Timestamp == other.Timestamp
                && new Span<byte>(this.Data).SequenceEqual(other.Data);
        }

        // https://blogs.msdn.microsoft.com/ericlippert/2011/02/28/guidelines-and-rules-for-gethashcode/
        public override int GetHashCode()
        {
            var fieldsHashCode = HashCodeHelper.CombineHashCodes(Timestamp.GetHashCode(),LinkLayer.GetHashCode());
            var arrayHashCode = HashCodeHelper.ArrayHashCode(this.Data);
            return HashCodeHelper.CombineHashCodes(fieldsHashCode, arrayHashCode);  
        }
    }
}
