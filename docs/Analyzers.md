# TARZAN.NFX.ANALYZERS

TARZAN.NFX.ANALYZERS contains a collection of basic functions to process  
packet captures loaded in Ignite Cluster. Namely, the following functions are 
available:

* Flow Analyzer - identifies all flows in the given captures.
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

## Service Detector
### USAGE:
```
dotnet tarzan.nfx.analyzers.dll detect-services --cache flowtable1  ... --cache flowtableN --method method1 ... --method methodN
```
### INPUT:
A collection of cache names that contains flows.
### OUTPUT:
Sets ```ApplicationName``` property of the flows. 

### Methods

* Port based detection

* Deep packet inspection, 
REF: https://github.com/ntop/nDPI/blob/dev/src/include/ndpi_typedefs.h

* 




## HTTP Analyzer
### INPUT



# Broadcasting Analyzers

Most of analyzers can be implemented as distributed compute action broadcasted to all data nodes. The analyzers use the principle of data collocation. 

The following example presents a simple action that merges specified input caches in a single output cache. Field ```m_ignite``` is supplied by the Ignite when the action is executed. To provide parameters for the analyzer, we use string names of input caches and output cache. 

```csharp
    public class MergeAnalyzer : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public string[] InputCacheNames { get; }

        public string OutputCacheName { get; }

        public MergeAnalyzer(string inputs, string output)
        {
            InputCacheNames = inputs;
            OutputCacheName = output;
        }

        public void Invoke()
        {
            var outputCache = m_ignite.GetOrCreateCache<object,object>(OutputCacheName);
            foreach(var cacheName in InputCacheNames)
            {
                var inputCache = m_ignite.GetOrCreateCache<object,object>(cacheName);
                outputCache.PutAll(inputCache.GetLocalEntries());
            }
        }
    }
```

The action can be executed by the Ignite client using the following code snippet:

```csharp
var compute = ignite.GetCompute();
var mergeAnalyzer = new MergeAnalyzer(flowCacheName, packetCacheNames, dnsOutCacheName);
compute.Broadcast(mergeAnalyzer);
```

Method ```Broadcast``` tells Ignite to send the computation to all cluster nodes. The compute action then can access items stored locally. If all nodes complete the computation, all entries are processed. 

# Data Access Patterns

## Queryable Flow Cache
Queryable flow cache can be obtained using ```CacheFactory```. Queryable cache enables 
to perform LINQ operations that are translated to IGNITE SQL and thus executed in a distributed 
fashion. It has also meaning for searching in local cache as it enables to use indexes. 
The following example returns all flows with for domain name service.

```csharp
var flowCache = CacheFactory.GetOrCreateFlowCache(ignite, FlowCacheName).AsCacheQueryable(local: true);
var dnsFlows = flowCache.Where(f => f.Value.ServiceName == "domain");
```

## Accessing Frames of the Flow
To get all frames for a flow use ```FrameCacheCollection``` class. The class implements 
cache query that enumerates all frames that belongs to the flow specified by its flow key. 
The following example gets all frames for the specified flow from the two frame caches, labeled as frames1 and frames2:

```csharp
var flowKey = FlowKey.Create(ProtocolType.Tcp, IPAddress.Parse("192.168.1.1"), 35346, IPAddress.Parse("147.229.11.100"), 80);
var frameCacheCollection = new FrameCacheCollection(ignite, new string[] { "frames1", "frames2" });
var frames = frameCacheCollection.GetFrames(flowkey);
```

## Conversations