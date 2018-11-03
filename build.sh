# build all projects using dotnet:
dotnet --list-sdks
dotnet build Tarzan.sln

# copy output from projects to artifacts folder:
cp -a src/Tarzan.Nfx.Analyzers/bin/Debug/netcoreapp2.0/* artifacts
cp -a src/Tarzan.Nfx.IgniteServer/bin/Debug/netcoreapp2.0/* artifacts
cp -a src/Tarzan.Nfx.PcapLoader/bin/Debug/netcoreapp2.0/* artifacts

