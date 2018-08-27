using Netdx.PacketDecoders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest
{
    /// <summary>
    /// Represents a raw captured link layer frame.
    /// </summary>
    public partial class Frame
    {                                     

            /// <value>
            /// The unix timestamp when the packet was created
            /// </value>
            public PosixTime UnixTimestamp
            {
                get => PosixTime.FromUnixTimeMilliseconds(_Timestamp);
            set => _Timestamp = value.ToUnixTimeMilliseconds();
            }
                       
            /// <summary> Gets data portion of the packet.</summary>
            public Span<byte> Bytes => _Data;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="LinkLayerType">
            /// A <see cref="LinkLayers"/>
            /// </param>
            /// <param name="Timeval">
            /// A <see cref="PosixTimeval"/>
            /// </param>
            /// <param name="Data">
            /// A <see cref="System.Byte"/>
            /// </param>
            public Frame(LinkLayerType linkLayerType,
                              PosixTime posixTime,
                              byte[] bytes)
            {
            this.LinkLayer = linkLayerType;
                this.Timestamp = posixTime.ToUnixTimeMilliseconds();
                this._Date = bytes;
            }

        /// <summary>
        /// Link-layer type codes. In fact, it is a copy of <see cref="PacketDotNet.LinkLayers"/>.
        /// </summary>
        public enum LinkLayerType : byte
        {
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

    }
}
