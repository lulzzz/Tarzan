using System;
using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;

namespace Tarzan.Nfx.Ingest
{
    class RecordProvider : IRecordProvider<(Packet, PosixTimeval), PacketStream>
    {
        public PacketStream GetRecord((Packet, PosixTimeval) capture)
        {
            return PacketStream.From(capture);
        }
    }    
}
