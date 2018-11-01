using Apache.Ignite.Core;
using Apache.Ignite.Core.Discovery.Tcp.Multicast;
using System;
using System.Collections.Generic;

namespace Tarzan.Nfx.Analyzers
{
    public class IgniteClient : IDisposable
    {
        const string DEFAULT_ENPOINTS = "127.0.0.1:47500";
        private readonly IgniteConfiguration m_cfg;
        private IIgnite m_ignite;
        public IgniteClient(ICollection<string> endpoints = null)
        {
            if (endpoints == null)
            {
                endpoints = new[] { DEFAULT_ENPOINTS };
            }
            m_cfg = new IgniteConfiguration
            {
                PeerAssemblyLoadingMode = Apache.Ignite.Core.Deployment.PeerAssemblyLoadingMode.CurrentAppDomain,
                DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryMulticastIpFinder
                    {
                        Endpoints = endpoints
                    }
                },
            };
        }

        public void SetEndpoints(ICollection<string> endpoints)
        {
            if (m_ignite != null) throw new InvalidOperationException("Cannot set endpoints for running client.");
            m_cfg.DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryMulticastIpFinder
                    {
                        Endpoints = endpoints
                    }
                };
        }

        public IIgnite Start()
        {
            Ignition.ClientMode = true;
            m_ignite = Ignition.Start(m_cfg);
            return m_ignite;
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
                disposedValue = true;
            }
        }

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
