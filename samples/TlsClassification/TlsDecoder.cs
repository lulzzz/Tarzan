using System;
using System.Collections;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Tarzan.Nfx.Packets.Common;

namespace Tarzan.Nfx.Samples.TlsClassification
{

    /// <summary>
    /// The decoder for TLS communication that can extract plain text from TLS communication 
    /// if premaster secret is known. See https://sharkfesteurope.wireshark.org/assets/presentations17eu/15.pdf
    /// </summary>
    public class TlsDecoder
    {
        /// <summary>
        /// Gets or sets the master secret.
        /// </summary>
        /// <value>The master secret.</value>
        public byte[] MasterSecret { get; set; }
        /// <summary>
        /// Gets or sets the client random.
        /// </summary>
        /// <value>The client random.</value>
        public byte[] ClientRandom { get; set; }
        /// <summary>
        /// Gets or sets the server random.
        /// </summary>
        /// <value>The server random.</value>
        public byte[] ServerRandom { get; set; }
        /// <summary>
        /// Gets or sets the protocol version.
        /// </summary>
        /// <value>The protocol version.</value>
        public SslProtocols ProtocolVersion { get; set; }
        /// <summary>
        /// Gets or sets the cipher suite.
        /// </summary>
        /// <value>The cipher suite.</value>
        public TlsCipherSuite CipherSuite { get; set; }

        /// <summary>
        /// Gets or sets the security parameters.
        /// </summary>
        /// <value>The security parameters.</value>
        public TlsSecurityParameters SecurityParameters { get; set; }

        /// <summary>
        /// Gets the key block.
        /// </summary>
        /// <value>The key block.</value>
        public TlsKeyBlock KeyBlock { get; private set;}

        public TlsDecoder()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:TlsClassification.TlsDecoder"/> class.
        /// </summary>
        /// <param name="masterSecret">Master secret bytes, lenght is 48.</param>
        /// <param name="clientRandom">Client random bytes, length is 32.</param>
        /// <param name="serverRandom">Server random bytes, length is 32.</param>
        public TlsDecoder(byte[] masterSecret, byte[] clientRandom, byte[] serverRandom)
        {
            MasterSecret = masterSecret;
            ClientRandom = clientRandom;
            ServerRandom = serverRandom;
        }


        public void InitializeKeyBlock(TlsSecurityParameters securityParameters)
        {
            SecurityParameters = securityParameters;
            // key_block = PRF(SecurityParameters.master_secret, "key expansion",
            // SecurityParameters.server_random + SecurityParameters.client_random);
            var bytes = securityParameters.PrfAlgorithm.GetSecretBytes(MasterSecret, "key expansion",
                                                                       ByteString.Combine(ServerRandom, ClientRandom),
                                                                       securityParameters.KeyMaterialSize/8);
                                                                       
            KeyBlock = new TlsKeyBlock(bytes, securityParameters.MacKeyLength, securityParameters.EncodingKeyLength, securityParameters.FixedIVLength);
        }


        /// <summary>
        /// Decodes the AES-CCM encrypted data according to RFC6655
        /// (https://tools.ietf.org/html/rfc6655).
        /// </summary>
        /// <returns>The decoded plain text or null.</returns>
        /// <param name="keyBytes">Key bytes.</param>
        /// <param name="initializationVector">Initialization vector.</param>
        /// <param name="sequenceNumber">Sequence number.</param>
        /// <param name="cipherBytes">Cipher text.</param>
        public static byte[] DecodeAesCcm128(Span<byte> keyBytes, Span<byte> nonceBytes, Span<byte> encryptedBytes, Span<byte> additionalData)
        {
            var aes = new AesLightEngine();
            var ccm = new CcmBlockCipher(aes);
            var key = new KeyParameter(keyBytes.ToArray());

            ccm.Init(false, new AeadParameters(key, 128, nonceBytes.ToArray(), additionalData.ToArray()));

            var outsize = ccm.GetOutputSize(encryptedBytes.Length);
            var plainBytes = new byte[outsize];
            var outBytes = ccm.ProcessBytes(encryptedBytes.ToArray(), 0, encryptedBytes.Length, plainBytes, 0);

            outBytes += ccm.DoFinal(plainBytes, outBytes);
            return plainBytes;
        }
        /// <summary>
        /// Decodes the AES-GCM encrypted data according to RFC5288 and RFC5264.
        /// (https://tools.ietf.org/html/rfc5288, https://tools.ietf.org/html/rfc5246#page-24).
        /// </summary>
        public static byte[] DecryptAesGcm128(Span<byte> writeKeyBytes, Span<byte> nonceBytes, Span<byte> encryptedBytes, Span<byte> additionalData)
        {

            var aes = new AesEngine();
            var gcm = new GcmBlockCipher(aes);
            var key = new KeyParameter(writeKeyBytes.ToArray());

            gcm.Init(false, new AeadParameters(key, 128, nonceBytes.ToArray(), additionalData.ToArray()));

            var outsize = gcm.GetOutputSize(encryptedBytes.Length);
            var plainBytes = new byte[outsize];
            var outBytes = gcm.ProcessBytes(encryptedBytes.ToArray(), 0, encryptedBytes.Length, plainBytes, 0);
            // finally check MAC:
            outBytes += gcm.DoFinal(plainBytes, outBytes);
            return plainBytes;
        }


        public byte[] DecryptApplicationData(TlsKeys tlsKeys, TlsPacket.TlsApplicationData applicationData, ulong sequenceNumber)
        {
            if (KeyBlock == null) throw new InvalidOperationException($"KeyBlock not initialized. Please, call {nameof(InitializeKeyBlock)} first.");
            var writeKey = tlsKeys.EncodingKey;

            var fixedNonce = new Span<byte>(tlsKeys.IV).Slice(0, SecurityParameters.FixedIVLength / 8);

            var content = new Span<byte>(applicationData.Body);

            var macLength = SecurityParameters.MacLength / 8;
            var recordNonceLength = SecurityParameters.RecordIVLength / 8;
            var recordNonce = content.Slice(0, recordNonceLength);

            var nonce = ByteString.Combine(fixedNonce.ToArray(), recordNonce.ToArray());

            var additionalData = ByteString.Combine(
                BitConverter.GetBytes(sequenceNumber).Reverse().ToArray(),
                new byte[] { (byte)applicationData.M_Parent.ContentType,
                applicationData.M_Parent.Version.Major,
                applicationData.M_Parent.Version.Minor }, 
                BitConverter.GetBytes((ushort)(applicationData.Body.Length - (recordNonceLength + macLength))).Reverse().ToArray()
            );
            return DecryptAesGcm128(writeKey, nonce, content.Slice(recordNonceLength), additionalData);
        }
    }
}
