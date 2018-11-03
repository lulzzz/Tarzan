# Nfx.PcapLoader

PcapLoader is implemented as a client and performs a single operation of loading pcap from a file or multiple pcap files from a folder to the Ignite Nfx Cluster.

## Usage
The following commands load specified files to the Nfx Cluster.  
```
dotnet Tarzan.Nfx.PcapLoader --cluster [IGNITE_CLUSTER_NODE] --mode [LOADER_MODE] --file [PCAP_FILES]
```

```
dotnet Tarzan.Nfx.PcapLoader --cluster [IGNITE_CLUSTER_NODE] --mode [LOADER_MODE] --folder [PCAP_FOLDER]
```

The mandatory arguments have the following meaning:

* IGNITE_CLUSTER_NODE - a string representing enpoint of any NFX cluster node. 
It can be domain name or ip address optionally followed by port number. For instance, 
`localhost:10800`. Default port is 10800.

* LOADER_MODE - specifies the opration of the loader. Loader can store data in the cache by put operation or can use streaming, which is far more faster. 
                Possible values are ```put```, ```stream``` or ```verify```. 

* PCAP_FILES - a list of pcap files to be loaded to the NFX cluster

* PCAP_FOLDER - a folder that contains PCAP files to be loaded to the NFX cluster.

There are some other options:

* ```-s|--chunk``` - Specifies the size of processing block. Frames are loaded, processed and stored in the cache in chunks. The default value is 100. 
            var chunkSizeArgument = commandLineApplication.Option("-s|--chunk", "A size of processing chunk. Packets are loaded and processed in chunks.", CommandOptionType.SingleValue);

* ```--disableProgressBar``` - By defalt the progress bar is shown, which may be not useful or wanted in some situations. This option disables showint the progress bar.
