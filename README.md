# TARZAN.NFX: A Network Forensix Platform

TARZAN.NFX is a distributed network forensics platform that aims to provide common functionality expected from a modern network forensic tool. The platform employs a distributed architecture that enables scaling and extensibility.
The following technologies form the basic environment for implementation of the TARZAN.NFX:

* Apache Ignite -- provides the environment for caching datasets and performing distributed computations mainly when collocation of data and computation is possible. 

* Apache Spark -- provides a scalable data processing engine suitable for specific types of operations.

* Cassandra -- provides persistence layer for data in rest.
