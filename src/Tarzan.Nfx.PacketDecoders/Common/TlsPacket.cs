// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace Tarzan.Nfx.Packets.Common
{
    public partial class TlsPacket : KaitaiStruct
    {
        public static TlsPacket FromFile(string fileName)
        {
            return new TlsPacket(new KaitaiStream(fileName));
        }


        public enum TlsContentType
        {
            ChangeCipherSpec = 20,
            Alert = 21,
            Handshake = 22,
            ApplicationData = 23,
        }

        public enum TlsHandshakeType
        {
            HelloRequest = 0,
            ClientHello = 1,
            ServerHello = 2,
            NewSessionTicket = 4,
            Certificate = 11,
            ServerKeyExchange = 12,
            CertificateRequest = 13,
            ServerHelloDone = 14,
            CertificateVerify = 15,
            ClientKeyExchange = 16,
            Finished = 20,
        }
        public TlsPacket(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _contentType = ((TlsContentType) m_io.ReadU1());
            _version = new TlsVersion(m_io, this, m_root);
            _length = m_io.ReadU2be();
            switch (ContentType) {
            case TlsContentType.Handshake: {
                __raw_fragment = m_io.ReadBytes(Length);
                var io___raw_fragment = new KaitaiStream(__raw_fragment);
                _fragment = new TlsHandshake(io___raw_fragment, this, m_root);
                break;
            }
            case TlsContentType.ApplicationData: {
                __raw_fragment = m_io.ReadBytes(Length);
                var io___raw_fragment = new KaitaiStream(__raw_fragment);
                _fragment = new TlsApplicationData(io___raw_fragment, this, m_root);
                break;
            }
            case TlsContentType.ChangeCipherSpec: {
                __raw_fragment = m_io.ReadBytes(Length);
                var io___raw_fragment = new KaitaiStream(__raw_fragment);
                _fragment = new TlsChangeCipherSpec(io___raw_fragment, this, m_root);
                break;
            }
            case TlsContentType.Alert: {
                __raw_fragment = m_io.ReadBytes(Length);
                var io___raw_fragment = new KaitaiStream(__raw_fragment);
                _fragment = new TlsEncryptedMessage(io___raw_fragment, this, m_root);
                break;
            }
            default: {
                __raw_fragment = m_io.ReadBytes(Length);
                var io___raw_fragment = new KaitaiStream(__raw_fragment);
                _fragment = new TlsEncryptedMessage(io___raw_fragment, this, m_root);
                break;
            }
            }
        }
        public partial class ServerName : KaitaiStruct
        {
            public static ServerName FromFile(string fileName)
            {
                return new ServerName(new KaitaiStream(fileName));
            }

            public ServerName(KaitaiStream p__io, TlsPacket.Sni p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _nameType = m_io.ReadU1();
                _length = m_io.ReadU2be();
                _hostName = m_io.ReadBytes(Length);
            }
            private byte _nameType;
            private ushort _length;
            private byte[] _hostName;
            private TlsPacket m_root;
            private TlsPacket.Sni m_parent;
            public byte NameType { get { return _nameType; } }
            public ushort Length { get { return _length; } }
            public byte[] HostName { get { return _hostName; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.Sni M_Parent { get { return m_parent; } }
        }
        public partial class Random : KaitaiStruct
        {
            public static Random FromFile(string fileName)
            {
                return new Random(new KaitaiStream(fileName));
            }

            public Random(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _gmtUnixTime = m_io.ReadU4be();
                _randomBytes = m_io.ReadBytes(28);
            }
            private uint _gmtUnixTime;
            private byte[] _randomBytes;
            private TlsPacket m_root;
            private KaitaiStruct m_parent;
            public uint GmtUnixTime { get { return _gmtUnixTime; } }
            public byte[] RandomBytes { get { return _randomBytes; } }
            public TlsPacket M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TlsCertificateRequest : KaitaiStruct
        {
            public static TlsCertificateRequest FromFile(string fileName)
            {
                return new TlsCertificateRequest(new KaitaiStream(fileName));
            }

            public TlsCertificateRequest(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _empty = m_io.ReadBytes(0);
            }
            private byte[] _empty;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public byte[] Empty { get { return _empty; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class TlsCertificate : KaitaiStruct
        {
            public static TlsCertificate FromFile(string fileName)
            {
                return new TlsCertificate(new KaitaiStream(fileName));
            }

            public TlsCertificate(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _certLength = new TlsLength(m_io, this, m_root);
                __raw_certificates = new List<byte[]>();
                _certificates = new List<Certificate>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        __raw_certificates.Add(m_io.ReadBytes(CertLength.Value));
                        var io___raw_certificates = new KaitaiStream(__raw_certificates[__raw_certificates.Count - 1]);
                        _certificates.Add(new Certificate(io___raw_certificates, this, m_root));
                        i++;
                    }
                }
            }
            private TlsLength _certLength;
            private List<Certificate> _certificates;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            private List<byte[]> __raw_certificates;
            public TlsLength CertLength { get { return _certLength; } }
            public List<Certificate> Certificates { get { return _certificates; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
            public List<byte[]> M_RawCertificates { get { return __raw_certificates; } }
        }
        public partial class Certificate : KaitaiStruct
        {
            public static Certificate FromFile(string fileName)
            {
                return new Certificate(new KaitaiStream(fileName));
            }

            public Certificate(KaitaiStream p__io, TlsPacket.TlsCertificate p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _certLength = new TlsLength(m_io, this, m_root);
                _body = m_io.ReadBytes(CertLength.Value);
            }
            private TlsLength _certLength;
            private byte[] _body;
            private TlsPacket m_root;
            private TlsPacket.TlsCertificate m_parent;
            public TlsLength CertLength { get { return _certLength; } }
            public byte[] Body { get { return _body; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsCertificate M_Parent { get { return m_parent; } }
        }
        public partial class SessionId : KaitaiStruct
        {
            public static SessionId FromFile(string fileName)
            {
                return new SessionId(new KaitaiStream(fileName));
            }

            public SessionId(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _len = m_io.ReadU1();
                _sid = m_io.ReadBytes(Len);
            }
            private byte _len;
            private byte[] _sid;
            private TlsPacket m_root;
            private KaitaiStruct m_parent;
            public byte Len { get { return _len; } }
            public byte[] Sid { get { return _sid; } }
            public TlsPacket M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class Sni : KaitaiStruct
        {
            public static Sni FromFile(string fileName)
            {
                return new Sni(new KaitaiStream(fileName));
            }

            public Sni(KaitaiStream p__io, TlsPacket.Extension p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _listLength = m_io.ReadU2be();
                _serverNames = new List<ServerName>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _serverNames.Add(new ServerName(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private ushort _listLength;
            private List<ServerName> _serverNames;
            private TlsPacket m_root;
            private TlsPacket.Extension m_parent;
            public ushort ListLength { get { return _listLength; } }
            public List<ServerName> ServerNames { get { return _serverNames; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.Extension M_Parent { get { return m_parent; } }
        }
        public partial class TlsServerHello : KaitaiStruct
        {
            public static TlsServerHello FromFile(string fileName)
            {
                return new TlsServerHello(new KaitaiStream(fileName));
            }

            public TlsServerHello(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _version = new TlsVersion(m_io, this, m_root);
                _random = new Random(m_io, this, m_root);
                _sessionId = new SessionId(m_io, this, m_root);
                _cipherSuite = new CipherSuite(m_io, this, m_root);
                _compressionMethods = new CompressionMethods(m_io, this, m_root);
                if (M_Io.IsEof == false) {
                    _extensions = new Extensions(m_io, this, m_root);
                }
            }
            private TlsVersion _version;
            private Random _random;
            private SessionId _sessionId;
            private CipherSuite _cipherSuite;
            private CompressionMethods _compressionMethods;
            private Extensions _extensions;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public TlsVersion Version { get { return _version; } }
            public Random Random { get { return _random; } }
            public SessionId SessionId { get { return _sessionId; } }
            public CipherSuite CipherSuite { get { return _cipherSuite; } }
            public CompressionMethods CompressionMethods { get { return _compressionMethods; } }
            public Extensions Extensions { get { return _extensions; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class CipherSuites : KaitaiStruct
        {
            public static CipherSuites FromFile(string fileName)
            {
                return new CipherSuites(new KaitaiStream(fileName));
            }

            public CipherSuites(KaitaiStream p__io, TlsPacket.TlsClientHello p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _len = m_io.ReadU2be();
                _cipherSuiteList = new List<ushort>((int) ((Len / 2)));
                for (var i = 0; i < (Len / 2); i++)
                {
                    _cipherSuiteList.Add(m_io.ReadU2be());
                }
            }
            private ushort _len;
            private List<ushort> _cipherSuiteList;
            private TlsPacket m_root;
            private TlsPacket.TlsClientHello m_parent;
            public ushort Len { get { return _len; } }
            public List<ushort> CipherSuiteList { get { return _cipherSuiteList; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsClientHello M_Parent { get { return m_parent; } }
        }
        public partial class TlsClientKeyExchange : KaitaiStruct
        {
            public static TlsClientKeyExchange FromFile(string fileName)
            {
                return new TlsClientKeyExchange(new KaitaiStream(fileName));
            }

            public TlsClientKeyExchange(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _exchangeKeys = m_io.ReadBytesFull();
            }
            private byte[] _exchangeKeys;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public byte[] ExchangeKeys { get { return _exchangeKeys; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class TlsChangeCipherSpec : KaitaiStruct
        {
            public static TlsChangeCipherSpec FromFile(string fileName)
            {
                return new TlsChangeCipherSpec(new KaitaiStream(fileName));
            }

            public TlsChangeCipherSpec(KaitaiStream p__io, TlsPacket p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _changeMessage = m_io.ReadBytesFull();
            }
            private byte[] _changeMessage;
            private TlsPacket m_root;
            private TlsPacket m_parent;
            public byte[] ChangeMessage { get { return _changeMessage; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket M_Parent { get { return m_parent; } }
        }
        public partial class CompressionMethods : KaitaiStruct
        {
            public static CompressionMethods FromFile(string fileName)
            {
                return new CompressionMethods(new KaitaiStream(fileName));
            }

            public CompressionMethods(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _len = m_io.ReadU1();
                _bytes = m_io.ReadBytes(Len);
            }
            private byte _len;
            private byte[] _bytes;
            private TlsPacket m_root;
            private KaitaiStruct m_parent;
            public byte Len { get { return _len; } }
            public byte[] Bytes { get { return _bytes; } }
            public TlsPacket M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TlsCertificateVerify : KaitaiStruct
        {
            public static TlsCertificateVerify FromFile(string fileName)
            {
                return new TlsCertificateVerify(new KaitaiStream(fileName));
            }

            public TlsCertificateVerify(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _empty = m_io.ReadBytes(0);
            }
            private byte[] _empty;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public byte[] Empty { get { return _empty; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class Alpn : KaitaiStruct
        {
            public static Alpn FromFile(string fileName)
            {
                return new Alpn(new KaitaiStream(fileName));
            }

            public Alpn(KaitaiStream p__io, TlsPacket.Extension p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _extLen = m_io.ReadU2be();
                _alpnProtocols = new List<Protocol>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _alpnProtocols.Add(new Protocol(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private ushort _extLen;
            private List<Protocol> _alpnProtocols;
            private TlsPacket m_root;
            private TlsPacket.Extension m_parent;
            public ushort ExtLen { get { return _extLen; } }
            public List<Protocol> AlpnProtocols { get { return _alpnProtocols; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.Extension M_Parent { get { return m_parent; } }
        }
        public partial class Extensions : KaitaiStruct
        {
            public static Extensions FromFile(string fileName)
            {
                return new Extensions(new KaitaiStream(fileName));
            }

            public Extensions(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _len = m_io.ReadU2be();
                _extensionList = new List<Extension>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _extensionList.Add(new Extension(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private ushort _len;
            private List<Extension> _extensionList;
            private TlsPacket m_root;
            private KaitaiStruct m_parent;
            public ushort Len { get { return _len; } }
            public List<Extension> ExtensionList { get { return _extensionList; } }
            public TlsPacket M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TlsPreMasterSecret : KaitaiStruct
        {
            public static TlsPreMasterSecret FromFile(string fileName)
            {
                return new TlsPreMasterSecret(new KaitaiStream(fileName));
            }

            public TlsPreMasterSecret(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _secretLength = m_io.ReadU2be();
                _secret = m_io.ReadBytes(SecretLength);
            }
            private ushort _secretLength;
            private byte[] _secret;
            private TlsPacket m_root;
            private KaitaiStruct m_parent;
            public ushort SecretLength { get { return _secretLength; } }
            public byte[] Secret { get { return _secret; } }
            public TlsPacket M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TlsServerKeyExchange : KaitaiStruct
        {
            public static TlsServerKeyExchange FromFile(string fileName)
            {
                return new TlsServerKeyExchange(new KaitaiStream(fileName));
            }

            public TlsServerKeyExchange(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _empty = m_io.ReadBytes(0);
            }
            private byte[] _empty;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public byte[] Empty { get { return _empty; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class TlsApplicationData : KaitaiStruct
        {
            public static TlsApplicationData FromFile(string fileName)
            {
                return new TlsApplicationData(new KaitaiStream(fileName));
            }

            public TlsApplicationData(KaitaiStream p__io, TlsPacket p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _body = m_io.ReadBytesFull();
            }
            private byte[] _body;
            private TlsPacket m_root;
            private TlsPacket m_parent;
            public byte[] Body { get { return _body; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket M_Parent { get { return m_parent; } }
        }
        public partial class TlsClientHello : KaitaiStruct
        {
            public static TlsClientHello FromFile(string fileName)
            {
                return new TlsClientHello(new KaitaiStream(fileName));
            }

            public TlsClientHello(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _version = new TlsVersion(m_io, this, m_root);
                _random = new Random(m_io, this, m_root);
                _sessionId = new SessionId(m_io, this, m_root);
                _cipherSuites = new CipherSuites(m_io, this, m_root);
                _compressionMethods = new CompressionMethods(m_io, this, m_root);
                if (M_Io.IsEof == false) {
                    _extensions = new Extensions(m_io, this, m_root);
                }
            }
            private TlsVersion _version;
            private Random _random;
            private SessionId _sessionId;
            private CipherSuites _cipherSuites;
            private CompressionMethods _compressionMethods;
            private Extensions _extensions;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public TlsVersion Version { get { return _version; } }
            public Random Random { get { return _random; } }
            public SessionId SessionId { get { return _sessionId; } }
            public CipherSuites CipherSuites { get { return _cipherSuites; } }
            public CompressionMethods CompressionMethods { get { return _compressionMethods; } }
            public Extensions Extensions { get { return _extensions; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class TlsServerHelloDone : KaitaiStruct
        {
            public static TlsServerHelloDone FromFile(string fileName)
            {
                return new TlsServerHelloDone(new KaitaiStream(fileName));
            }

            public TlsServerHelloDone(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _empty = m_io.ReadBytes(0);
            }
            private byte[] _empty;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public byte[] Empty { get { return _empty; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class TlsEncryptedMessage : KaitaiStruct
        {
            public static TlsEncryptedMessage FromFile(string fileName)
            {
                return new TlsEncryptedMessage(new KaitaiStream(fileName));
            }

            public TlsEncryptedMessage(KaitaiStream p__io, TlsPacket p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _encryptedMessage = m_io.ReadBytesFull();
            }
            private byte[] _encryptedMessage;
            private TlsPacket m_root;
            private TlsPacket m_parent;
            public byte[] EncryptedMessage { get { return _encryptedMessage; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket M_Parent { get { return m_parent; } }
        }
        public partial class CipherSuite : KaitaiStruct
        {
            public static CipherSuite FromFile(string fileName)
            {
                return new CipherSuite(new KaitaiStream(fileName));
            }

            public CipherSuite(KaitaiStream p__io, TlsPacket.TlsServerHello p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _cipherId = m_io.ReadU2be();
            }
            private ushort _cipherId;
            private TlsPacket m_root;
            private TlsPacket.TlsServerHello m_parent;
            public ushort CipherId { get { return _cipherId; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsServerHello M_Parent { get { return m_parent; } }
        }
        public partial class TlsEmpty : KaitaiStruct
        {
            public static TlsEmpty FromFile(string fileName)
            {
                return new TlsEmpty(new KaitaiStream(fileName));
            }

            public TlsEmpty(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _empty = m_io.ReadBytes(0);
            }
            private byte[] _empty;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public byte[] Empty { get { return _empty; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class TlsHandshake : KaitaiStruct
        {
            public static TlsHandshake FromFile(string fileName)
            {
                return new TlsHandshake(new KaitaiStream(fileName));
            }

            public TlsHandshake(KaitaiStream p__io, TlsPacket p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _msgType = ((TlsPacket.TlsHandshakeType) m_io.ReadU1());
                if ((int)MsgType < 32) {
                    _length = new TlsLength(m_io, this, m_root);
                }
                if ((int)MsgType < 32) {
                    switch (MsgType) {
                    case TlsPacket.TlsHandshakeType.HelloRequest: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsEmpty(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.Certificate: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsCertificate(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.CertificateVerify: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsCertificateVerify(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.ServerKeyExchange: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsServerKeyExchange(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.ClientHello: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsClientHello(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.Finished: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsFinished(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.ClientKeyExchange: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsClientKeyExchange(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.ServerHello: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsServerHello(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.CertificateRequest: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsCertificateRequest(io___raw_body, this, m_root);
                        break;
                    }
                    case TlsPacket.TlsHandshakeType.ServerHelloDone: {
                        __raw_body = m_io.ReadBytes(Length.Value);
                        var io___raw_body = new KaitaiStream(__raw_body);
                        _body = new TlsServerHelloDone(io___raw_body, this, m_root);
                        break;
                    }
                    default: {
                        _body = m_io.ReadBytes(Length.Value);
                        break;
                    }
                    }
                }
                _encryptedMsg = m_io.ReadBytesFull();
            }
            private TlsHandshakeType _msgType;
            private TlsLength _length;
            private object _body;
            private byte[] _encryptedMsg;
            private TlsPacket m_root;
            private TlsPacket m_parent;
            private byte[] __raw_body;
            public TlsHandshakeType MsgType { get { return _msgType; } }
            public TlsLength Length { get { return _length; } }
            public object Body { get { return _body; } }
            public byte[] EncryptedMsg { get { return _encryptedMsg; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket M_Parent { get { return m_parent; } }
            public byte[] M_RawBody { get { return __raw_body; } }
        }
        public partial class Protocol : KaitaiStruct
        {
            public static Protocol FromFile(string fileName)
            {
                return new Protocol(new KaitaiStream(fileName));
            }

            public Protocol(KaitaiStream p__io, TlsPacket.Alpn p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _strlen = m_io.ReadU1();
                _name = m_io.ReadBytes(Strlen);
            }
            private byte _strlen;
            private byte[] _name;
            private TlsPacket m_root;
            private TlsPacket.Alpn m_parent;
            public byte Strlen { get { return _strlen; } }
            public byte[] Name { get { return _name; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.Alpn M_Parent { get { return m_parent; } }
        }
        public partial class TlsLength : KaitaiStruct
        {
            public static TlsLength FromFile(string fileName)
            {
                return new TlsLength(new KaitaiStream(fileName));
            }

            public TlsLength(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_value = false;
                _read();
            }
            private void _read()
            {
                _hlen = m_io.ReadU1();
                _llen = m_io.ReadU2be();
            }
            private bool f_value;
            private int _value;
            public int Value
            {
                get
                {
                    if (f_value)
                        return _value;
                    _value = (int) ((Llen + (Hlen << 16)));
                    f_value = true;
                    return _value;
                }
            }
            private byte _hlen;
            private ushort _llen;
            private TlsPacket m_root;
            private KaitaiStruct m_parent;
            public byte Hlen { get { return _hlen; } }
            public ushort Llen { get { return _llen; } }
            public TlsPacket M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TlsVersion : KaitaiStruct
        {
            public static TlsVersion FromFile(string fileName)
            {
                return new TlsVersion(new KaitaiStream(fileName));
            }

            public TlsVersion(KaitaiStream p__io, KaitaiStruct p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _major = m_io.ReadU1();
                _minor = m_io.ReadU1();
            }
            private byte _major;
            private byte _minor;
            private TlsPacket m_root;
            private KaitaiStruct m_parent;
            public byte Major { get { return _major; } }
            public byte Minor { get { return _minor; } }
            public TlsPacket M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class TlsFinished : KaitaiStruct
        {
            public static TlsFinished FromFile(string fileName)
            {
                return new TlsFinished(new KaitaiStream(fileName));
            }

            public TlsFinished(KaitaiStream p__io, TlsPacket.TlsHandshake p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _verifyData = m_io.ReadBytesFull();
            }
            private byte[] _verifyData;
            private TlsPacket m_root;
            private TlsPacket.TlsHandshake m_parent;
            public byte[] VerifyData { get { return _verifyData; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.TlsHandshake M_Parent { get { return m_parent; } }
        }
        public partial class Extension : KaitaiStruct
        {
            public static Extension FromFile(string fileName)
            {
                return new Extension(new KaitaiStream(fileName));
            }

            public Extension(KaitaiStream p__io, TlsPacket.Extensions p__parent = null, TlsPacket p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _type = m_io.ReadU2be();
                _len = m_io.ReadU2be();
                switch (Type) {
                case 0: {
                    __raw_body = m_io.ReadBytes(Len);
                    var io___raw_body = new KaitaiStream(__raw_body);
                    _body = new Sni(io___raw_body, this, m_root);
                    break;
                }
                case 16: {
                    __raw_body = m_io.ReadBytes(Len);
                    var io___raw_body = new KaitaiStream(__raw_body);
                    _body = new Alpn(io___raw_body, this, m_root);
                    break;
                }
                default: {
                    _body = m_io.ReadBytes(Len);
                    break;
                }
                }
            }
            private ushort _type;
            private ushort _len;
            private object _body;
            private TlsPacket m_root;
            private TlsPacket.Extensions m_parent;
            private byte[] __raw_body;
            public ushort Type { get { return _type; } }
            public ushort Len { get { return _len; } }
            public object Body { get { return _body; } }
            public TlsPacket M_Root { get { return m_root; } }
            public TlsPacket.Extensions M_Parent { get { return m_parent; } }
            public byte[] M_RawBody { get { return __raw_body; } }
        }
        private TlsContentType _contentType;
        private TlsVersion _version;
        private ushort _length;
        private KaitaiStruct _fragment;
        private TlsPacket m_root;
        private KaitaiStruct m_parent;
        private byte[] __raw_fragment;
        public TlsContentType ContentType { get { return _contentType; } }
        public TlsVersion Version { get { return _version; } }
        public ushort Length { get { return _length; } }
        public KaitaiStruct Fragment { get { return _fragment; } }
        public TlsPacket M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
        public byte[] M_RawFragment { get { return __raw_fragment; } }
    }
}
