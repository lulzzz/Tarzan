using Apache.Ignite.Core;
using Apache.Ignite.Core.Discovery.Tcp.Multicast;
using Apache.Ignite.NLog;
using NLog;
using System;
using System.Collections.Generic;

namespace Tarzan.Nfx.Analyzers
{
    public class IgniteClient : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
                Logger = new IgniteNLogLogger()
            };
        }

        public void SetEndpoints(ICollection<string> endpoints)
        {
            if (m_ignite != null) throw new InvalidOperationException("Cannot set endpoints for running client.");

            logger.Debug($"Set TcpDiscoverySpi endpoints: {String.Join(',',endpoints)}.");
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
            logger.Trace("Starting Ignite Client...");
            m_ignite = Ignition.Start(m_cfg);
            logger.Trace("...Ignite Client started.");
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
                    logger.Trace("Disposing Ignite Client...");
                    m_ignite.Dispose();
                    logger.Trace("...Ignite Client disposed.");

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
