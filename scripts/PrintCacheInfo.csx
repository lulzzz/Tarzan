#r "C:\GitHub\Tarzan\packages\ignite2.8.0-alpha\lib\netcoreapp2.0\Apache.Ignite.Core.dll"
using Apache.Ignite.Core;
using Apache.Ignite.Core.Deployment;
using Apache.Ignite.Core.Discovery.Tcp.Static;

Ignition.ClientMode = true;
var cfg = new IgniteConfiguration
{

    PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,
    DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
    {
        IpFinder = new TcpDiscoveryStaticIpFinder
        {
            Endpoints = new[] { "127.0.0.1:47500" }
        },
    }, 
    IgniteHome = @"C:\GitHub\Tarzan\packages\ignite2.8.0-alpha" 
};
using (var client = Ignition.Start(cfg))
{
    var cacheNames = client.GetCacheNames();
    foreach(var cacheName in cacheNames)
    {
        var cache = client.GetCache<object, object>(cacheName);
        Console.WriteLine($"Cache '{cache.Name}': {cache.GetSize()} items");
    }
}