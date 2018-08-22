﻿// Compile with:          
// thrift --gen csharp -o Tarzan.Nfx.Model model.thrift 

namespace csharp Tarzan.Nfx.Model

struct PacketFlow {
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

//--- APPLICATIONS ------------------------------------------------------
struct DnsInfo {
    // Refers to flow containing answer packet
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

// Holds information about a single HTTP Request/response.
struct HttpInfo {
    // Refers to flow containing request packet
    1: string FlowId; 
    2: string TransactionId;    
    4: i64 Timestamp;
    5: string Client;
    6: string Server;
    11: string Method;
    12: string Host;
    13: string Uri;
    14: string Referrer;
    15: string Version;
    16: string UserAgent;
    17: string Username;
    18: string Password;
    21: string StatusCode;
    22: string StatusMessage;
    31: list<string> RequestHeaders;
    32: list<string> ResponseHeaders;
    33: i32 RequestBodyLength;
    34: i32 ResponseBodyLength;
    35: list<binary> RequestBodyChunks;
    36: list<binary> ResponseBodyChunks;
    37: string RequestContentType;
    38: string ResponseContentType;
}
struct HttpInfoFilter {
    1: string Uri;
    2: list<string> ContentTypeList;
    3: string AtLeastSize;
    4: string AtMostSize;
    5: string TimeRange;    
}