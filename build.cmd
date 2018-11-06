dotnet --list-sdks
dotnet build Tarzan.sln

REM copy output from projects to artifacts folder:
copy /Y src\Tarzan.Nfx.Analyzers\bin\Debug\netcoreapp2.0\* artifacts
copy /Y src\Tarzan.Nfx.IgniteServer\bin\Debug\netcoreapp2.0\* artifacts
copy /Y src\Tarzan.Nfx.PcapLoader\bin\Debug\netcoreapp2.0\* artifacts