Measure-Command {
$p0 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-000.pcap" -PassThru -Verb open
$p1 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-001.pcap" -PassThru -Verb open
$p2 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-002.pcap" -PassThru -Verb open
$p3 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-003.pcap" -PassThru -Verb open
$p4 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-004.pcap" -PassThru -Verb open
$p5 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-005.pcap" -PassThru -Verb open
$p6 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-006.pcap" -PassThru -Verb open
$p7 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-007.pcap" -PassThru -Verb open
$p8 = Start-Process -FilePath "dotnet.exe" -ArgumentList "Tarzan.Nfx.PcapLoader.dll --cluster 127.0.0.1 --file D:\Captures\ids\testbed-12jun-512\testbed-12jun-008.pcap" -PassThru -Verb open

Wait-Process -Id $p0.Id,$p1.Id,$p2.Id,$p3.Id,$p4.Id,$p5.Id,$p6.Id,$p7.Id,$p8.Id
}