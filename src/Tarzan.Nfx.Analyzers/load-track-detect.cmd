rem Load -> Track-Flows -> Detect-Services  
dotnet Tarzan.Nfx.PcapLoader.dll -Cluster 127.0.0.1:47500 -SourceFile d:\Captures\ids\testbed-12jun-128\testbed-12jun-000.pcap -Mode stream
dotnet Tarzan.Nfx.Analyzers.dll -Cluster  127.0.0.1 Track-Flows -PacketCache testbed-12jun-000.pcap -WriteTo testbed-12jun-000.flow
dotnet Tarzan.Nfx.Analyzers.dll -Cluster  127.0.0.1 Detect-Services -FlowCache testbed-12jun-000.flow