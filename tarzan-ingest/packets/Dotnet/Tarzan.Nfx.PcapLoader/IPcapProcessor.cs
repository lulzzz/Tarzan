using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Tarzan.Nfx.PcapLoader
{
    public interface IPcapProcessor
    {
        int ChunkSize { get; set; }
        IPEndPoint ClusterNode { get; set; }
        IList<FileInfo> SourceFiles { get; }

        event ChunkCompletedHandler OnChunkLoaded;
        event ChunkCompletedHandler OnChunkStored;
        event FileCompletedHandler OnFileCompleted;
        event FileOpenHandler OnFileOpen;

        Task Invoke();
    }
}