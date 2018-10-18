@REM RECREATES IGNITE CLUSTER FROM 4 NODES AND SETS UP TOPOLOGY
RMDIR /S c:\Apache\Ignite\work
@REM START NODES
START dotnet Tarzan.Nfx.IgniteServer.dll
TIMEOUT 10
START dotnet Tarzan.Nfx.IgniteServer.dll 
TIMEOUT 10
START dotnet Tarzan.Nfx.IgniteServer.dll 
TIMEOUT 10
START dotnet Tarzan.Nfx.IgniteServer.dll
TIMEOUT 20
@REM ACTIVATE CLUSTER
ignitecontrol --activate
@REM CREATE TOPOLOGY
C:\Apache\Ignite\bin\control --baseline version 4
@REM ALL DONE
