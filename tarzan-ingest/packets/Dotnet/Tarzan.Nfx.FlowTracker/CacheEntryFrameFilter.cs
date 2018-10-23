using Apache.Ignite.Core.Cache;
using System;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;

namespace Tarzan.Nfx.Analyzers
{
    /// <summary>
    /// This filter selects all frames of the specified flow.
    /// </summary>  
    [Serializable]
    public class CacheEntryFrameFilter : ICacheEntryFilter<FrameKey, Frame>
    {
        public FlowKey FlowKey { get; set; }
        public IKeyProvider<FlowKey, Frame> KeyProvider { get; set; }

        public CacheEntryFrameFilter(FlowKey flowKey)
        {
            FlowKey = flowKey;
            KeyProvider = new FrameKeyProvider();
        }

        public CacheEntryFrameFilter()
        {
        }

        public bool Invoke(ICacheEntry<FrameKey, Frame> frame)
        {
            return FlowKey.HashCode != frame.Key.FlowKeyHash ? false : FlowKey.Equals(KeyProvider.GetKey(frame.Value));
        }
    }
}
