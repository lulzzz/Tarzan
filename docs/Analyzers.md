# TARZAN.NFX.ANALYZERS

TARZAN.NFX.ANALYZERS contains a collection of basic functions to process  
packet captures loaded in Ignite Cluster. Namely, the following functions are 
available:

* Flow Analyzer - determines netflows in captures
* Application Detector - identifies applications or services for flows. 
* DNS Analyzer - extracts information from DNS flows.
* HTTP Analyzer - extracts information from HTTP flows, including content.
* TLS Analyzer - extracts information from TLS flows that can be used for further analysis.  

These functions are accessible from CLI by invoking corresponding commands. The 
application defines some common settings:

* ```-c|--cluster``` *Enpoint string of any cluster node. It is possible to specify multiple nodes by repeating the option.* 
* ```-t|--trace``` *Prints various debug information.*

## Flow Analyzer
Flow analyzer process all frames in the specified source caches and computes 
flow objects. It stores flows in the given cache. 

Usage:

```dotnet Tarzan.Nfx.Analyzers.dll [settings] track-flows [arguments] [options]```

Required arguments:

* ```-r|--read``` *Name of the cache with frames to read from. Multiple values can be specified.*
* ```-w|--write``` *Name of the (possibly fresh) cache to write flows to.*

Options:


Example:

In the example, all flows in source cache named `testbed-12jun-000.pcap` are identified and stored to cache `testbed-12jun-000.flows`. The connection string is 
`127.0.0.1:47500`. Connection string must specify Discovery Spi port of at least one node of a cluster:
```
dotnet Tarzan.Nfx.Analyzers.dll --cluster 127.0.0.1:47500 track-flows -read testbed-12jun-000.pcap --write testbed-12jun-000.flows 
```

## Application Detector
### USAGE:
```
dotnet tarzan.nfx.analyzers.dll application --cache flowtable1  ... --cache flowtableN --method method1 ... --method methodN
```
### INPUT:
A collection of cache names that contains flows.
### OUTPUT:
Sets ```ApplicationName``` property of the flows. 

## HTTP Analyzer
### INPUT
