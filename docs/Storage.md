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
    notes text,
    tags set<text>
);
```

### Flows
```cql
CREATE TABLE flows (
    id uuid PRIMARY KEY, 
    protocols set<text>, 
    sourceAddress inet, 
    sourcePort int, 
    destinationAddress inet, 
    destinationPort int, 
    firstSeen timestamp, 
    lastSeen timestamp, 
    packets int, 
    octets bigint
);
```

### Hosts
```cql
CREATE TABLE hosts (
    address inet PRIMARY KEY,
    hostname text, 
    sessions int,
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
### DNS
```cql

CREATE TYPE dns_record (
  class text,
  type text,
  name text
);

CREATE TABLE dns (
    flowId uuid PRIMARY KEY,    
    question list<dns_record>,
    answer list<dns_record>,
    authority list<dns_record>,
    additional list<dns_record>
);
```
