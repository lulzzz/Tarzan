# Apache Ignite

## Requirements
Apache Ignite requires Java 8 to be installed. The latest release can be found here: https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html

## Baseline Topology
If persistence is enabled, nodes that have persistente memory region in the cluster are organized in baseline topology. 

Such cluster needs to be activated for the first use:

* Run all nodes

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
Current topology version: 6

Baseline nodes:
    ConsistentID=32050db0-718e-4b2d-bad1-022c69ce8f99, STATE=ONLINE
    ConsistentID=b4d9a129-b7f1-4f3a-802c-fadf46a3cac3, STATE=ONLINE
    ConsistentID=bdb04771-138d-44d5-8ad1-d35fe272e956, STATE=ONLINE
--------------------------------------------------------------------------------
Number of baseline nodes: 3

Other nodes:
    ConsistentID=e7ee9ac9-329a-478c-a48b-a9dccdc600c5
Number of other nodes: 1
Press any key to continue . . .

```

* Add nodes that are not included in the baseline

```
$control --baseline add e7ee9ac9-329a-478c-a48b-a9dccdc600c5
Control utility [ver. 2.6.0#20180710-sha1:669feacc]
2018 Copyright(C) Apache Software Foundation
User: John Doe
--------------------------------------------------------------------------------
Warning: the command will perform changes in baseline.
Press 'y' to continue . . . y
Cluster state: active
Current topology version: 6

Baseline nodes:
    ConsistentID=32050db0-718e-4b2d-bad1-022c69ce8f99, STATE=ONLINE
    ConsistentID=b4d9a129-b7f1-4f3a-802c-fadf46a3cac3, STATE=ONLINE
    ConsistentID=bdb04771-138d-44d5-8ad1-d35fe272e956, STATE=ONLINE
    ConsistentID=e7ee9ac9-329a-478c-a48b-a9dccdc600c5, STATE=ONLINE
--------------------------------------------------------------------------------
Number of baseline nodes: 4

Other nodes not found.
```

## Links

* Examples: https://github.com/apache/ignite/tree/master/examples/src/main/java/org/apache/ignite/examples
