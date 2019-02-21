# Apache Ignite

## Requirements
As of this writing, the currently available Ignite nuget version is 2.6 (https://www.nuget.org/packages/Apache.Ignite/).
This version is built against the Java 8 and .NET Framework 4.6 and .NET Core 2.0. While most of the 
features work with newer version, for full functionality the correct Java and .NET versions are necessary.

### Java 
Apache Ignite requires Java 8 to be installed. The latest release of Java 8 is labeled
as  Java SE Development Kit 8u192 and can be downloaded from here: https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html 

### .NET Core SDK 
The .NET is compiled against .NET Framework v4.6. and .NET Core 2.0. 
It is thus necessary to have a correct version of .NET runtime and SDK. When .NET Core > 2.0 is used 
some strange runtime errors may happend. The correct version is .NET Core 2.0 and it is avialable from the 
archive: https://www.microsoft.com/net/download/dotnet-core/2.0 Note that the last .NET Core 2.0 runtime is denoted as 2.0.9 and 
the correspoding SDK is 2.1.209.

## Deployment

* Download Ignite Binary Distribution from https://ignite.apache.org/download.cgi

* Extract file to destination folder `C:\Apache\Ignite\apache-ignite-fabric-2.6.0-bin`

* Add system variable: `IGNITE_HOME=C:\Apache\Ignite\apache-ignite-fabric-2.6.0-bin`

* Add binary directory of Ignite to PATH: `%IGNITE_HOME%\bin`

* Check that system contains `JAVA_HOME` variable and it is properly set: `JAVA_HOME=C:\Program Files\Java\jdk1.8.0_192`

## Baseline Topology
If persistence is enabled, nodes that have persistente memory region in the cluster are organized in baseline topology. 

Such cluster needs to be activated for the first use:

* Run all nodes:
```
START dotnet Tarzan.Nfx.IgniteServer.dll -f config\persistent-config.xml -g 1024 -h 1024 -p 47500 -i tarzan_head_node
START dotnet Tarzan.Nfx.IgniteServer.dll -f config\persistent-config.xml -g 1024 -h 1024 -p 47501 -i tarzan_other_node1
START dotnet Tarzan.Nfx.IgniteServer.dll -f config\persistent-config.xml -g 1024 -h 1024 -p 47502 -i tarzan_other_node2
START dotnet Tarzan.Nfx.IgniteServer.dll -f config\persistent-config.xml -g 1024 -h 1024 -p 47503 -i tarzan_other_node3
```

* Perform activation of the cluster

```bash
$control --activate
Control utility [ver. 2.6.0-20180710-sha1:669feacc]
2018 Copyright(C) Apache Software Foundation
User: John Doe
--------------------------------------------------------------------------------
Cluster activated
Press any key to continue . . .
```

* Check baseline

```bash
$control --baseline
Control utility [ver. 2.6.0-20180710-sha1:669feacc]
2018 Copyright(C) Apache Software Foundation
User: John Doe
Cluster state: active
Current topology version: 4

Baseline nodes:
    ConsistentID=tarzan_head_node, STATE=ONLINE
   
--------------------------------------------------------------------------------
Number of baseline nodes: 1

Other nodes:
    ConsistentID=tarzan_other_node1
    ConsistentID=tarzan_other_node2
    ConsistentID=tarzan_other_node3
Number of other nodes: 3
Press any key to continue . . .

```

* Add nodes that are not included in the baseline

```
$control --baseline add tarzan_other_node1,tarzan_other_node2,tarzan_other_node3
Control utility [ver. 2.6.0#20180710-sha1:669feacc]
2018 Copyright(C) Apache Software Foundation
User: John Doe
--------------------------------------------------------------------------------
Warning: the command will perform changes in baseline.
Press 'y' to continue . . . y
Cluster state: active
Current topology version: 6

Baseline nodes:
    ConsistentID=tarzan_head_node, STATE=ONLINE
    ConsistentID=tarzan_other_node1, STATE=ONLINE
    ConsistentID=tarzan_other_node2, STATE=ONLINE
    ConsistentID=tarzan_other_node3, STATE=ONLINE
--------------------------------------------------------------------------------
Number of baseline nodes: 4

Other nodes not found.
```

## Loading assemblies on-demand
First, all nodes must be executed with configuration that enables assembly loading:

```csharp
var cfg = new IgniteConfiguration
{
    PeerAssemblyLoadingMode = Apache.Ignite.Core.Deployment.PeerAssemblyLoadingMode.CurrentAppDomain
};
```
```xml
<igniteConfiguration peerAssemblyLoadingMode='CurrentAppDomain' >
...
</igniteConfiguration>
```

Auto assembly loading does not work as expected for streaming and data processors. In this case the required assembly is not loaded and
thus streaming may not functioned properly. Either the error occured explaining that the  type of cache processor instance is not found or 
the application just hung. 

The workaround is simple:
For every assembly that should be loaded to the server create an empty action class. 
Then on start up when this dummy action is broadcasted , it causes the loading of the containing assembly. 

```
class AssemblyLoadingAction : IComputeAction
{
    public static string Assembly = Assemlby
    public void Invoke()
    {
        Console.WriteLine($"Loading assembly {m_message}");
    }
}
```


## ETL Workload

* Load Frames:

```
dotnet Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --folder D:\Captures\ids\testbed-12jun-512 --mode stream
```


* Track flows:

```
dotnet Tarzan.Nfx.FlowTracker.dll -s testbed-12jun-000.pcap -s testbed-12jun-001.pcap -s testbed-12jun-002.pcap -s testbed-12jun-003.pcap -s testbed-12jun-004.pcap -s testbed-12jun-005.pcap -s testbed-12jun-006.pcap -s testbed-12jun-007.pcap -s testbed-12jun-008.pcap
```



## Links

* Examples: https://github.com/apache/ignite/tree/master/examples/src/main/java/org/apache/ignite/examples
