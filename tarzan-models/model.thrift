// Compile with:          
// thrift --gen csharp -o Tarzan.Nfx.Model model.thrift 

namespace csharp Tarzan.Nfx.Model

// Represents an AFF4 Object in the Object Catalogue. 
// Objects are addresable by their name which is unique. 
// Name of the object uses URN notation.
struct AffObject {
    // Name of the object using URN notation, for example, "urn:aff4:83a3d6db-85d5-4730-8216-1987853bc4d2".
    1: string ObjectName;
    // Type of the object, for example, "PacketFlow".
    2: string ObjectType;
    // Name of the interface to access the object. It can be null if default interface derived from ObjectType is to be used. 
    3: string Interface;
    // Location of the object. It can be null if the object is stored in the implicit location (Cassandra cluster).
    4: string Stored;
}
// Represents a single AFF4 statement. It is a tripple (Subject, Attribute, Value).
struct AffStatement {
    1: string Subject;
    2: string Attribute;
    3: string Value;
}

struct Capture {
  1: string Uid;
  2: string Name;
  3: i64 CreationTime;
  4: i64 Length;
  5: string Hash;
}

// Object namespace: urn:aff4:flows/
// Object name: UID.Generate[FirstSeen, Protocol, SourceAddress, SourcePort, DestinationAddress, DestinationPort]
struct PacketFlow { 
    1: string FlowUid;
    2: string Protocol;
    3: string SourceAddress;
    4: i32 SourcePort;
    5: string DestinationAddress;
    6: i32 DestinationPort;
    7: i64 FirstSeen;
    8: i64 LastSeen;
    9: i32 Packets;
    10: i64 Octets;
    14: string ServiceName;
}

// Object namespace:  urn:aff4:hosts/
// Object name: [Address]
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

// Object namespace: urn:aff4:services/
// Object name:  [SERVICE NAME]
struct Service {
    1: string ServiceName;
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
// Object namespace: urn:aff4:dns/
// Object name: [FlowUid]/[TransactionId]
struct DnsObject {
    1: string FlowUid;    
    2: string TransactionId;
    5: i64 Timestamp;
    6: string Client;
    7: string Server;
    8: i32 DnsTtl;
    9: string DnsType;
    10: string DnsQuery;
    11: string DnsAnswer;
}

// Holds information about a single HTTP Request/response.
// Object namespace: urn:aff4:http/   
// Object name:     [FlowUid]/[ObjectIndex]
struct HttpObject {    
    // Refers to flow containing request packet
    1: string FlowUid; 
    2: string ObjectIndex;    
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