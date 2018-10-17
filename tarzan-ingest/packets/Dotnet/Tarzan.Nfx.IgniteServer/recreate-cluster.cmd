@REM RECREATES IGNITE CLUSTER FROM 4 NODES AND SETS UP TOPOLOGY
RMDIR /S c:\Apache\Ignite\work
@REM START NODES
START dotnet Tarzan.Nfx.IgniteServer.dll 
START dotnet Tarzan.Nfx.IgniteServer.dll 
START dotnet Tarzan.Nfx.IgniteServer.dll 
START dotnet Tarzan.Nfx.IgniteServer.dll 
@REM ACTIVATE CLUSTER
C:\Apache\Ignite\bin\control --activate
@REM CREATE TOPOLOGY
C:\Apache\Ignite\bin\control --baseline version 4
@REM ALL DONE
