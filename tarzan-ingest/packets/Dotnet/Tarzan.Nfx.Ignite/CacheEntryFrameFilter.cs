using Apache.Ignite.Core.Cache;
using System;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;

namespace Tarzan.Nfx.Ignite
{
    /// <summary>
    /// Implements <see cref="ICacheEntryFilter{FrameKey, Frame}"/> filter that 
    /// selects frames for the given <see cref="Model.FlowKey"/>. 
    /// To use the filter, do not forget to add it to the BinaryConfiguration, 
    /// because this filter is sent over the wire to remote nodes and needs to be serialized.
    /// </summary>
    [Serializable]
    public class CacheEntryFrameFilter : ICacheEntryFilter<FrameKey, FrameData>
    {
        public FlowKey FlowKey { get; set; }
        public IKeyProvider<FlowKey, FrameData> KeyProvider { get; set; }

        public CacheEntryFrameFilter(FlowKey flowKey)
        {
            FlowKey = flowKey;
            KeyProvider = new FrameKeyProvider();
        }

        public CacheEntryFrameFilter()
        {
        }

        public bool Invoke(ICacheEntry<FrameKey, FrameData> frame)
        {
            return FlowKey.HashCode != frame.Key.FlowKeyHash ? false : FlowKey.Equals(KeyProvider.GetKey(frame.Value));
        }
    }
}
