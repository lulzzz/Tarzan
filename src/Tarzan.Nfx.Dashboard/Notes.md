# Developer's Notes

* We use Cassandra in Docker: https://docs.docker.com/samples/library/cassandra/


To run the Cassandra container use:

```
docker run -d -p 9042:9042 --name netdx_db -d cassandra
```
This command exposes port 9042 to be accessible from the local machine. To connect the docket machine just type:
```
docker exec -it netdx_db /bin/bash
#cqlsh
```


# Outlook
The structure of the application:
User Management Module

Case Management Module
- Cases: case management is separated module from the analytical module. When the cases is open the application switch to analytical module.

Analytical Module:
- Captures
- Flows: Table, Map, ...
- Views: Hosts, Files, Images, Messages, Credentials, Sessions, Domain Names, Parameters, SSL/TLS
- Results: Keywords, Anomalies, ...

Creating ASP.NET REST API Backend
* https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-vsc?view=aspnetcore-2.1