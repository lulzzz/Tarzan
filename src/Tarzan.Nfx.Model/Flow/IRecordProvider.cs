namespace Tarzan.Nfx.Model
{
    public interface IRecordProvider<TPacket, TFlowRecord>
    {
        TFlowRecord GetRecord(TPacket packet);
    }
}