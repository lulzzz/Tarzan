using System;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public class TlsKeyBlock
    {
        private readonly byte[] bytes;
        readonly int macSize;
        readonly int keySize;
        readonly int ivSize;

        public TlsKeyBlock(byte[] bytes, int macSize, int keySize, int ivSize)
        {
            this.ivSize = ivSize;
            this.keySize = keySize;
            this.macSize = macSize;
            this.bytes = bytes;
        }
        public Span<byte> ClientWriteMacSecret => new Span<byte>(bytes).Slice(0, macSize);
        public Span<byte> ServerWriteMacSecret => new Span<byte>(bytes).Slice(macSize, macSize);
        public Span<byte> ClientWriteKey => new Span<byte>(bytes).Slice(macSize + macSize, keySize);
        public Span<byte> ServerWriteKey => new Span<byte>(bytes).Slice(macSize + macSize + keySize, keySize);
        public Span<byte> ClientIV => new Span<byte>(bytes).Slice(macSize + macSize + keySize + keySize, ivSize);
        public Span<byte> ServerIV => new Span<byte>(bytes).Slice(macSize + macSize + keySize + keySize + ivSize, ivSize);
    }
}
