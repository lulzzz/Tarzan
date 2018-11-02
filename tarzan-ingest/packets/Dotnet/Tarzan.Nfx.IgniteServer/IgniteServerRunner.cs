using Apache.Ignite.Core;
using Apache.Ignite.Core.Configuration;
using Apache.Ignite.Core.Deployment;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Multicast;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tarzan.Nfx.IgniteServer
{
    public class IgniteServerRunner : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();        
        private IgniteConfiguration m_igniteConfiguration;

        public IIgnite Ignite { get; private set; }

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
            m_igniteConfiguration.Logger = new IgniteNLogLogger();
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
            if (Ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
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
            if (Ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
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
            if (Ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
            m_igniteConfiguration.JvmMaxMemoryMb = value;
        }
        public void SetOffHeapMemoryLimit(int value)
        {
            if (Ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
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
            if (Ignite != null) throw new InvalidOperationException("Cannot configure running instances.");
            m_igniteConfiguration.ConsistentId = cid;
        }

        private void PrintConfiguration()
        {

        }

        public async Task Run()
        {
            var tcs = new TaskCompletionSource<string>();
            logger.Info("Starting Ignite Server...");
            Ignite = Ignition.Start(m_igniteConfiguration);
            var cts = new CancellationTokenSource();
            Ignite.Stopped += (s, e) => tcs.SetResult(e.ToString());
            var localSpidPort = (Ignite.GetConfiguration().DiscoverySpi as TcpDiscoverySpi).LocalPort;
            logger.Info($"Ignite server is running (Local SpiDiscovery Port={localSpidPort}), press CTRL+C to terminate.");
            await tcs.Task;
            logger.Info("Ignite server stopped.");
        }

        public void Terminate()
        {            
            Ignition.Stop(Ignite.Name, true);
        }
        public void Stop()
        {
            Ignition.Stop(Ignite.Name, false);            
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Ignite.Dispose();    
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
