@ECHO Running %1 instances of Ignite Server
@FOR /L %%G IN (1,1,%1) DO START dotnet Tarzan.Nfx.Ingest.dll start-ignite

REM START dotnet Tarzan.Nfx.Ingest.dll track-flows -folder D:\Captures\ids\testbed-12jun-128 -cassandra "localhost" -namespace testbed -create