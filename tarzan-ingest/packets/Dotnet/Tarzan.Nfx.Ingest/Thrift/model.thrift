// compile with:
// thrift --gen csharp model.thrift 
namespace csharp Tarzan.Nfx.Ingest

struct Frame {
	1: i64 Timestamp;
	2: i16 LinkLayer
	3: binary Data;
}

struct PacketStream {    
    1: i64 FirstSeen;
    2: i64 LastSeen;
    3: i64 Octets;
    4: i32 Packets;
	5: list<Frame> FrameList;
}
