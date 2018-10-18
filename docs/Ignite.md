# Apache Ignite

## Requirements
Apache Ignite requires Java 8 to be installed. The latest release can be found here: https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html 

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

## Running compute jobs on cluster nodes
The compute jobs are represented by classes that can be disseminated on demands.
The following needs to be satisfied in order to run compute jobs that relies on package distribution:

* All nodes must be executed with configuration that enables to assembly loading:

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

* If new version of assembly is available the old one needs to be removed from the storage.

## Links

* Examples: https://github.com/apache/ignite/tree/master/examples/src/main/java/org/apache/ignite/examples
