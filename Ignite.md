# Apache Ignite



## Baseline
If persistence is enabled, nodes that have persistente memory region in the cluster are organized in baseline topology. 

Such cluster needs to be activated for the first use:
* Run all nodes
* Perform activation of the cluster
```
$control --activate
```
* Check baseline
```
$control --baseline


```
* Add nodes that are not included in the baseline


## Links

* Examples: https://github.com/apache/ignite/tree/master/examples/src/main/java/org/apache/ignite/examples
