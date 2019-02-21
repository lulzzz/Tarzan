using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Datastream;
using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model.Observable;

namespace Tarzan.Nfx.PcapLoader.PacketFlow
{
    [Serializable]
    public sealed class PacketFlowVisitor : IStreamReceiver<string, Artifact>
    {
        private MergePacketFlowProcessor m_updateProcessor;
        public PacketFlowVisitor(MergePacketFlowProcessor updateProcessor)
        {
            this.m_updateProcessor = updateProcessor;
        }
        public void Receive(ICache<string, Artifact> cache, ICollection<ICacheEntry<string, Artifact>> entries)
        {
            Console.Write('.');
            foreach (var entry in entries)
            {
                cache.Invoke(entry.Key, m_updateProcessor, entry.Value);
            }
        }
    }
}
