@ECHO Running %1 instances of Ignite Server
@FOR /L %%G IN (1,1,%1) DO START dotnet Tarzan.Nfx.IgniteServer.dll 