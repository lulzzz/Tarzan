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
                this.Data = bytes;
            }
    }
}
