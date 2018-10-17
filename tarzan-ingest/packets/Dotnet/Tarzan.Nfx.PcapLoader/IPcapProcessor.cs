using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.PcapLoader
{
    public delegate void FileOpenHandler(object sender, FileInfo fileInfo);
    public delegate void FileCompletedHandler(object sender, FileInfo fileInfo);
    public delegate void ChunkCompletedHandler(object sender, int chunkNumber, int chunkBytes);
    public delegate void ErrorFrameHandler(object sender, FileInfo fileInfo, int frameNumber, Frame frame);
    public interface IPcapProcessor
    {
        int ChunkSize { get; set; }
        IPEndPoint ClusterNode { get; set; }
        IList<FileInfo> SourceFiles { get; }

        event ChunkCompletedHandler OnChunkLoaded;
        event ChunkCompletedHandler OnChunkStored;
        event FileCompletedHandler OnFileCompleted;
        event FileOpenHandler OnFileOpen;
        event ErrorFrameHandler OnErrorFrame;

        Task Invoke();
    }
}