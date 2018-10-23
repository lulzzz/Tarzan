using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Analyzers
{
    public class FlowRecord<TFlowData>
    {
        public PacketFlow Flow { get; set; }
        public TFlowData Data { get; set; }
    }
}
