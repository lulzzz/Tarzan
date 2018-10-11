# Nfx.PcapLoader

PcapLoader is implemented as an Ignite Thin client and performs a single operation of loading pcap from a file or multiple pcap files from a folder to the Ignite Nfx Cluster.


## Usage
The following commands loads specified files to the Nfx Cluster.  
```
dotnet Tarzan.Nfx.PcapLoader -cluster [IGNITE_CLUSTER_NODE] -file [PCAP_FILES]
```
```
dotnet Tarzan.Nfx.PcapLoader -cluster [IGNITE_CLUSTER_NODE] -folder [PCAP_FOLDER]
```
* IGNITE_CLUSTER_NODE - a string representing enpoint of any NFX cluster node. 
It can be domain name or ip address optionally followed by port number. For instance, 
`localhost:10800`. Default port is 10800.

* PCAP_FILES - a list of pcap files to be loaded to the NFX cluster

* PCAP_FOLDER - a folder that contains PCAP files to be loaded to the NFX cluster.
