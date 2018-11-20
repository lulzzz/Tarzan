using System;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public class TlsKeyBlock
    {
        private readonly byte[] bytes;
        readonly int macKeySize;
        readonly int enckeySize;
        readonly int ivecSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Tarzan.Nfx.Samples.TlsClassification.TlsKeyBlock"/> class.
        /// </summary>
        /// <param name="bytes">Bytes.</param>
        /// <param name="macKeySize">Mac key size in bits.</param>
        /// <param name="encKeySize">Enc key size in bits.</param>
        /// <param name="ivecSize">Ivec size in bits.</param>
        public TlsKeyBlock(byte[] bytes, int macKeySize, int encKeySize, int ivecSize)
        {
            this.ivecSize = ivecSize;
            this.enckeySize = encKeySize;
            this.macKeySize = macKeySize;
            this.bytes = bytes;
        }
        public Span<byte> ClientWriteMacSecret => new Span<byte>(bytes).Slice(0, macKeySize/8);
        public Span<byte> ServerWriteMacSecret => new Span<byte>(bytes).Slice(macKeySize/8, macKeySize/8);
        public Span<byte> ClientWriteKey => new Span<byte>(bytes).Slice(macKeySize/8 + macKeySize/8, enckeySize/8);
        public Span<byte> ServerWriteKey => new Span<byte>(bytes).Slice(macKeySize/8 + macKeySize/8 + enckeySize/8, enckeySize/8);
        public Span<byte> ClientIV => new Span<byte>(bytes).Slice(macKeySize/8 + macKeySize/8 + enckeySize/8 + enckeySize/8, ivecSize/8);
        public Span<byte> ServerIV => new Span<byte>(bytes).Slice(macKeySize/8 + macKeySize/8 + enckeySize/8 + enckeySize/8 + ivecSize/8, ivecSize/8);

        public TlsKeys GetClientKeys()
        {
            return new TlsKeys
            {
                EncodingKey = ClientWriteKey.ToArray(),
                MacKey = ClientWriteMacSecret.ToArray(),
                IV = ClientIV.ToArray()
            };
        }
        public TlsKeys GetServerKeys()
        {
            return new TlsKeys
            {
                EncodingKey = ServerWriteKey.ToArray(),
                MacKey = ServerWriteMacSecret.ToArray(),
                IV = ServerIV.ToArray()
            };
        }

    }

    public class TlsKeys
    {
        public byte[] EncodingKey { get; set; }
        public byte[] MacKey { get; set; }
        public byte[] IV { get; set; }
    }
}
