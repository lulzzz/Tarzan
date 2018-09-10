using System;
namespace Tarzan.Nfx.Ingest.Flow
{

    /// <summary>
    /// Defines interface for key providers. The key provider returns 
    /// <see cref="FlowKey"/> for the packet.  
    /// </summary>
    public interface IKeyProvider<TFlowKey, TPacket>
    {
        TFlowKey GetKey(TPacket packet);        
    }
}
