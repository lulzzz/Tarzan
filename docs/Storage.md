# TARZAN NFX Storage

The primary data storage is served by Cassandra DB. 

## Cassandra 
Cassandra is a key-value possible distributed database. 
The primary key can span multiple columns. An efficient access 
to values is possible by using their keys. 

The primary key can be defined by several columns:
* Partition key helps Cassandra to quickly retrieve the data from the appropriate node.
* Clustering columns order data within a partition. The database uses the clustering information to identify where the data is within the partition.

## Database Schema 
The Cassandra DB is used for storing extracted data. When processing input pcap files
extended flow records are created. The flow records are extended with application specific data
and additional information on flows (entropy, TCP flags, and other relevant information).

### Extended Flows
Extended flow information is always related to the basic flow record. The basic flow record
is stored in `flows` table. Each flow has a unique identifier (uuid) which serves as the flow 
key. Each flow has an attribute that lists its protocols, e.g., `[ether, ip, tcp, http]`. This
information can be used to search for other relevant information specific for the particular protocol. Information related to http protocol is stored in `flows_http` table and can be find 
by searching for flow id. 




### Case 
First, we need to select a keyspace. The keyspace will correspond to the name of the case. 
```cql
CREATE KEYSPACE Case01 WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };
```
Then set the created case as the current keyspace:
```
USE Case01;
```
### Captures
```cql
CREATE TABLE captures (
    id uuid PRIMARY KEY, 
    name text,
    type text, 
    size int,
    createdOn timestamp, 
    uploadedOn timestamp, 
    hash text, 
    author text,
);
```

### Flows
```cql

CREATE TYPE ipendpoint (
    address inet,
    port int
);

CREATE TABLE flows (
    protocol text, 
    source frozen<ipendpoint>, 
    destination frozen<ipendpoint>, 
    flow_id uuid, 
    firstSeen timestamp, 
    lastSeen timestamp, 
    packets int, 
    octets bigint,
    layers set<text>,
    PRIMARY KEY ((protocol, source, destination), flowid)
);

CREATE INDEX ON flows(flowid);
```

### Flow Statistics
```cql
CREATE TABLE flows_stats (

);
```

### DNS
```cql
CREATE TYPE dns_record (
  class text,
  type text,
  name text
);

CREATE TABLE flows_dns (
    flowref uuid PRIMARY KEY,    
    question list<frozen<dns_record>>,
    answer list<frozen<dns_record>>,
    authority list<frozen<dns_record>>,
    additional list<frozen<dns_record>>
);
```

### HTTP 
```cql
CREATE TYPE http_request (

);


CREATE TYPE http_response (

);

CREATE TABLE flows_http (
    flowref uuid PRIMARY KEY, 

);
```

### TLS
```cql
CREATE TYPE tls_record (

);

CREATE TABLE flows_tls (
    flowref uuid PRIMARY KEY, 
);
```

### Hosts
```cql
CREATE TABLE hosts (
    address inet PRIMARY KEY,
    hostname text, 
    upflows int,
    downflows int,
    octetsSent bigint,
    octetsRecv bigint,
    packetsSent int,
    packetsRecv int  
);
```

### Services 
```cql
CREATE TABLE services (
    protocol text PRIMARY KEY,
    flows int,
    packetsTotal int, 
    packetsMin int,
    packetsMax int,
    octetsTotal bigint, 
    octetsMin int,
    octetsMax int,
    durationMin int,
    durationMax int,
    durationAvg int
);
```