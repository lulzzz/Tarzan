// compile with:
// thrift --gen csharp model.thrift 
namespace csharp Tarzan.Nfx.Ingest

enum LinkLayerType {
    /// <summary> no link-layer encapsulation </summary>
    Null = 0,

    /// <summary> Ethernet (10Mb) </summary>
    Ethernet = 1,

    /// <summary> Experimental Ethernet (3Mb) </summary>
    ExperimentalEthernet3MB = 2,

    /// <summary> Amateur Radio AX.25 </summary>
    AmateurRadioAX25 = 3,

    /// <summary> Proteon ProNET Token Ring </summary>
    ProteonProNetTokenRing = 4,

    /// <summary> Chaos </summary>
    Chaos = 5,

    /// <summary> IEEE 802 Networks </summary>
    Ieee802 = 6,

    /// <summary> ARCNET </summary>
    ArcNet = 7,

    /// <summary> Serial Line IP </summary>
    Slip = 8,

    /// <summary> Point-to-point Protocol </summary>
    Ppp = 9,

    /// <summary> FDDI </summary>
    Fddi = 10,

    /// <summary> LLC/SNAP encapsulated atm </summary>
    AtmRfc1483 = 11,

    /// <summary> raw IP </summary>
    Raw = 12,

    /// <summary> BSD Slip.</summary>
    SlipBSD = 15,

    /// <summary> BSD PPP.</summary>
    PppBSD = 16,

    /// <summary> IP over ATM.</summary>
    AtmClip = 19,

    /// <summary> PPP over HDLC.</summary>
    PppSerial = 50,

    /// <summary> Cisco HDLC.</summary>
    CiscoHDLC = 104,

    /// <summary> IEEE 802.11 wireless.</summary>
    Ieee80211 = 105,

    /// <summary> OpenBSD loopback.</summary>
    Loop = 108,

    /// <summary> Linux cooked sockets.</summary>
    LinuxSLL = 113,

    /// <summary>
    /// Header for 802.11 plus a number of bits of link-layer information
    /// including radio information, used by some recent BSD drivers as
    /// well as the madwifi Atheros driver for Linux.
    /// </summary>
    Ieee80211_Radio = 127,

    /// <summary>
    /// Per Packet Information encapsulated packets.
    /// DLT_ requested by Gianluca Varenni &lt;gianluca.varenni@cacetech.com&gt;.
    /// See http://www.cacetech.com/documents/PPI%20Header%20format%201.0.7.pdf
    /// </summary>
    PerPacketInformation = 192,
}

struct Frame {
	1: i64 Timestamp;
	2: LinkLayerType LinkLayer
	3: binary Data;
}

struct PacketStream {    
    1: i16 Protocol;
    2: binary SourceAddress;
    3: i32 SourcePort;
    4: binary DestinationAddress;
    5: i32 DestinationPort;
    10: i64 FirstSeen;
    11: i64 LastSeen;
    12: i64 Octets;
    13: i32 Packets;
    14: string ServiceName;
	20: list<Frame> FrameList;
}
