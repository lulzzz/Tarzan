using Apache.Ignite.Core;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Deployment;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Multicast;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tarzan.Nfx.IgniteServer
{
    class IgniteServerRunner : IDisposable
    {
        private ConsoleLogger m_logger = new ConsoleLogger("Tarzan.IgniteServer", (s, ll) => true, true);
        private IgniteConfiguration m_igniteConfiguration;

        public IgniteServerRunner(string configurationFile=null)
        {
            m_igniteConfiguration = new IgniteConfiguration();
            if (configurationFile != null)
            {
                m_igniteConfiguration = LoadConfiguration(configurationFile);
            }
            else
            {
                m_igniteConfiguration = GetDefaultConfiguration();
            }
        }

        private static IgniteConfiguration LoadConfiguration(string filename)
        {
            var configStr = File.ReadAllText(filename);
            var igniteConfiguration = IgniteConfiguration.FromXml(configStr);
            return igniteConfiguration;
        }

        private static IgniteConfiguration GetDefaultConfiguration()
        {
            var cfg = new IgniteConfiguration
            {
                JvmOptions = new[] { 
                                     "-XX:+AlwaysPreTouch",
                                     "-XX:+UseG1GC",
                                     "-XX:+ScavengeBeforeFullGC",
                                     "-XX:+DisableExplicitGC",
                },
                PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,
                DataStorageConfiguration = new DataStorageConfiguration(),
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryMulticastIpFinder
                    {
                        MulticastGroup = "228.10.10.157",
                        Endpoints = new []{ "127.0.0.1:47500" }
                    }
                }
            };
            return cfg;
        }

        public void SetClusterEnpoints(ICollection<string> values)
        {
            if (m_ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
            switch (m_igniteConfiguration.DiscoverySpi)
            {
                case TcpDiscoverySpi tcpDiscoverySpi:
                    tcpDiscoverySpi.IpFinder = new TcpDiscoveryStaticIpFinder { Endpoints = values }; break;
                case null:
                    m_igniteConfiguration.DiscoverySpi = new TcpDiscoverySpi { IpFinder = new TcpDiscoveryStaticIpFinder { Endpoints = values } }; break;
            }
        }

        public void SetServerPort(int value)
        {
            if (m_ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
            switch (m_igniteConfiguration.DiscoverySpi)
            {
                case TcpDiscoverySpi tcpDiscoverySpi:
                    tcpDiscoverySpi.LocalPort = value; break;
                case null:
                    m_igniteConfiguration.DiscoverySpi = new TcpDiscoverySpi { LocalPort = value }; break;
            }
        }

        public void SetOnHeapMemoryLimit(int value)
        {
            if (m_ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
            m_igniteConfiguration.JvmMaxMemoryMb = value;
        }
        public void SetOffHeapMemoryLimit(int value)
        {
            if (m_ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
            if (m_igniteConfiguration?.DataStorageConfiguration?.DefaultDataRegionConfiguration != null)
            {
                m_igniteConfiguration.DataStorageConfiguration.DefaultDataRegionConfiguration.MaxSize = (long)value * 1024 * 1024;
            }
            else
            {
                m_igniteConfiguration.DataStorageConfiguration.DefaultDataRegionConfiguration = new DataRegionConfiguration { Name="default", MaxSize = (long)value * 1024 * 1024 };
            }
        }

        public void SetPersistence(bool value)
        {
            if (m_igniteConfiguration?.DataStorageConfiguration?.DefaultDataRegionConfiguration != null)
            {
                m_igniteConfiguration.DataStorageConfiguration.DefaultDataRegionConfiguration.PersistenceEnabled = value;
            }
            else
            {
                m_igniteConfiguration.DataStorageConfiguration.DefaultDataRegionConfiguration = new DataRegionConfiguration { Name="default", PersistenceEnabled = value };
            }
        }
        public void SetConsistentId(string cid)
        {
            if (m_ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
            m_igniteConfiguration.ConsistentId = cid;
        }

        private void PrintConfiguration()
        {

        }

        IIgnite m_ignite;
        public async Task Run()
        {
            var tcs = new TaskCompletionSource<string>();
            m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Status", 0, $"Ignite server stopped.", null);

            m_ignite = Ignition.Start(m_igniteConfiguration);                                                                                                                                                                                                      
            var cts = new CancellationTokenSource();
            m_ignite.Stopped += (s, e) => tcs.SetResult(e.ToString());
            var localSpidPort = (m_ignite.GetConfiguration().DiscoverySpi as TcpDiscoverySpi).LocalPort;
            m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Status", 0, $"Ignite server is running (Local SpiDiscovery Port={localSpidPort}), press CTRL+C to terminate.", null);
            await tcs.Task;
            m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Status", 0, $"Ignite server stopped.", null);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_ignite.Dispose();    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IgniteServerRunner() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
