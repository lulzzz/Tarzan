// TO COMPILE:          
// thrift --gen csharp model.thrift 
namespace csharp Tarzan.Nfx.Model

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

struct Service {
1: string Name;
2: i32 Flows;
3: i32 Packets;
4: i32 MinPackets;
5: i32 MaxPackets;
6: i64 Octets;
7: i64 MinOctets;
8: i64 MaxOctets;
9: i64 MinDuration;
10: i64 MaxDuration;
11: i64 AvgDuration;
}

struct DnsInfo {
1: string FlowId;
2: string DnsId;
5: i64 Timestamp;
6: string Client;
7: string Server;
8: i32 DnsTtl;
9: string DnsType;
10: string DnsQuery;
11: string DnsAnswer;
}

struct HttpInfo {
1: string FlowId;
2: string Rid;
3: string Method;
4: string Host;
5: string Uri;
6: string Referrer;
7: string Version;
8: string UserAgent;
9: i32 RequestBodyLenght;
10: i32 ResponseBodyLength;
11: string StatusCode;
12: string StatusMessage;
13: string InfoCode;
14: string InfoMessage;
15: string Username;
16: string Password;
17: list<string> Headers;
}