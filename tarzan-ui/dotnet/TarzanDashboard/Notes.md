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



# ASP.NET + Angular Material
Some link to information about ASP.NET and Angular integration:

* https://material.angular.io/guide/getting-started
* https://developer.okta.com/blog/2018/04/26/build-crud-app-aspnetcore-angular
* https://www.c-sharpcorner.com/article/crud-operations-with-asp-net-core-using-angular-5-and-ado-net/
* https://mdbootstrap.com/angular/angular-project-structure/



Creating ASP.NET REST API Backend
* https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-vsc?view=aspnetcore-2.1