using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using Kaitai;
using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Packets.Common;
using Tarzan.Nfx.Packets.Core;

namespace Tarzan.Nfx.Ingest.Analyzers
{
    public class TlsAnalyzer : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;

        public string FlowCacheName { get; }

        public IEnumerable<string> FrameCacheNames { get; }

        public string TlsCacheName { get; }


        public TlsAnalyzer(string flowCacheName, IEnumerable<string> frameCacheNames, string tlsCacheName)
        {
            FlowCacheName = flowCacheName;
            FrameCacheNames = frameCacheNames;
            TlsCacheName = tlsCacheName;
        }

        private TlsPacket ParseTlsPacket(FrameData frame)
        {
            throw new NotImplementedException();
        }

        public void Invoke()
        {
            throw new NotImplementedException();
        }
    }
}