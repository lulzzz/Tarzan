// TO COMPILE:          
// thrift --gen csharp model.thrift 
namespace csharp Tarzan.UI.Server.Models

struct Flow {
1: string Protocol;
2: string SourceAddress;
3: i32 SourcePort;
4: string DestinationAddress;
5: i32 DestinationPort;
6: string FlowId;
7: i64 FirstSeen;
8: i64 LastSeen;
9: i32 Packets;
10: i64 Octets;
}

struct Host {
1: string Address;
2: string Hostname;
3: i32 UpFlows;
4: i32 DownFlows;
5: i64 OctetsSent;
6: i64 OctetsRecv;
7: i32 PacketsSent;
8: i32 PacketsRecv;
}