using System.Security.Authentication;

namespace Tarzan.Nfx.Samples.TlsClassification
{
    public class TlsSecurityParameters
    {
        public enum TlsCipherType { Stream, Block, Aead }
        public enum TlsCommpressionMethod { Null }

        public PrfAlgorithm PrfAlgorithm { get; set; }
        public CipherAlgorithmType CipherAlgorithm { get; set; }
        public TlsCipherType CipherType { get; set; }
        public int EncodingKeyLength { get; set; }
        public int BlockLength { get; set; }
        public int FixedIVLength { get; set; }
        public int RecordIVLength { get; set; }

        public HashAlgorithmType MacAlgorithm { get; set; }
        public int MacLength { get; set; }
        public int MacKeyLength { get; set; }

        public TlsCommpressionMethod CommpressionMethod { get; set; }

        public int KeyMaterialSize => MacKeyLength * 2
                                    + EncodingKeyLength * 2
                                    + FixedIVLength * 2;
    }
}
