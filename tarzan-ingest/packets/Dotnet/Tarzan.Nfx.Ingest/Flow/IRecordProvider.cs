namespace Tarzan.Nfx.Ingest.Flow
{
    public interface IRecordProvider<TPacket, TFlowRecord>
    {
        TFlowRecord GetRecord(TPacket packet);
    }
}