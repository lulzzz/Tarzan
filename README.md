# TARZAN.NFX: A Network Forensic Platform

TARZAN.NFX is a distributed network forensics platform that aims to provide common functionality expected from a modern network forensic tool. The platform employs a distributed architecture that enables scaling and extensibility.
The following technologies form the basic environment for implementation of the TARZAN.NFX:

* Apache Ignite -- provides the environment for caching datasets and performing distributed computations mainly when collocation of data and computation is possible. 

* Apache Spark -- provides a scalable data processing engine suitable for specific types of operations.

* Apache Flink -- real-time processing engine used for input stream processing. 

* Cassandra -- provides persistence layer for data in rest.


## Solution organization
The structure of the solution is organized in the following top level folders:

* src - Main projects (the product code)
* tests - Test projects
* docs - Documentation stuff, markdown files, help files etc.
* samples (optional) - Sample projects
* lib - Things that can NEVER exist in a nuget package
* artifacts - Build outputs go here. Doing a build.cmd/build.sh generates artifacts here (nupkgs, dlls, pdbs, etc.)
* packages - NuGet packages
* build - Build customizations (custom msbuild files) scripts

### Building
Currently, building the solution means just to run ```dotnet build Tarzan.sln```. By executing scripts build.cmd or
build.sh the solution is compiled and binary files are copied to ```artifacts```.
