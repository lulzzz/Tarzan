# Nfx.PcapLoader

PcapLoader is implemented as a client and performs a single operation of loading pcap from a file or multiple pcap files from a folder to the Ignite Nfx Cluster.

## Usage
The following commands load specified files to the Nfx Cluster.  
```
dotnet Tarzan.Nfx.PcapLoader -Cluster [IGNITE_CLUSTER_NODE] -Mode [LOADER_MODE] -SourceFile [PCAP_FILES] -WriteTo [FRAME_CHACHE]
```

```
dotnet Tarzan.Nfx.PcapLoader -Cluster [IGNITE_CLUSTER_NODE] -Mode [LOADER_MODE] -SourceFolder [PCAP_FOLDER] -WriteTo [FRAME_CHACHE]
```

The mandatory arguments have the following meaning:

* IGNITE_CLUSTER_NODE - a string representing enpoint of any NFX cluster node. 
It can be domain name or ip address optionally followed by port number. For instance, 
`localhost:10800`. Default port is 10800.

* LOADER_MODE - specifies the opration of the loader. Loader can store data in the cache by put operation or can use streaming, which is far more faster. 
                Possible values are ```put```, ```stream``` or ```verify```. 

* PCAP_FILES - a list of pcap files to be loaded to the NFX cluster

* PCAP_FOLDER - a folder that contains PCAP files to be loaded to the NFX cluster.

* FRAME_CACHE - a name of the cache used for storing loaded frames.

The optional parameters are as follows:

* ```-ChunkSize``` - Specifies the size of processing block. Frames are loaded, processed and stored in chunks. The default value is 100.
Depending on the hardware and cluster configuration the different value may lead to better (or poor) performance. 


* ```-DisableProgressBar``` - By defalt the progress bar is shown, which may be not useful or wanted in some situations. This option disables showint the progress bar.

## Examples
