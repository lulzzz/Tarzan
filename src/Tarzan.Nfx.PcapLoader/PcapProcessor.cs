using Apache.Ignite.Core;
using Apache.Ignite.Core.Deployment;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.NLog;
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
    public delegate void ErrorFrameHandler(object sender, FileInfo fileInfo, int frameNumber, FrameData frame);
    public abstract class PcapProcessor
    {
        protected static NLog.Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();

        public const int MAX_ON_HEAP = 1024;
        public const int MAX_OFF_HEAP = 1024;
        public const int DEFAULT_PORT = 47500;
        public const int DEFAULT_CHUNK_SIZE = 1000;

        public int ChunkSize { get; set; } = DEFAULT_CHUNK_SIZE;
        public IPEndPoint ClusterNode { get; set; } = new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT);
        public IList<FileInfo> SourceFiles { get; } = new List<FileInfo>();
        public string FrameCacheName { get; set; }

        public event ChunkCompletedHandler ChunkLoaded;
        public event ChunkCompletedHandler ChunkStored;
        public event FileCompletedHandler FileCompleted;
        public event FileOpenHandler FileOpened;
        public event ErrorFrameHandler ErrorFrameOccured;

        protected virtual void OnChunkLoaded(int chunkNumber, int chunkBytes)
        {
            ChunkLoaded?.Invoke(this, chunkNumber, chunkBytes);
        }
        protected virtual void OnChunkStored(int chunkNumber, int chunkBytes)
        {
            ChunkStored?.Invoke(this, chunkNumber, chunkBytes);
        }
        protected virtual void OnFileCompleted(FileInfo fileInfo)
        {
            FileCompleted?.Invoke(this, fileInfo);
        }
        protected virtual void OnFileOpened(FileInfo fileInfo)
        {
            FileOpened?.Invoke(this, fileInfo);
        }
        protected virtual void OnErrorFrameOccured(FileInfo fileInfo, int frameNumber, FrameData frame)
        {
            ErrorFrameOccured?.Invoke(this, fileInfo, frameNumber, frame);
        }

        public abstract Task Invoke();

        public IgniteConfiguration GetIgniteConfiguration()
        {
            var cfg = new IgniteConfiguration
            {
                JvmOptions = new[] { //"-Xms256m",
                                     $"-Xmx{MAX_ON_HEAP}m",
                                     "-XX:+AlwaysPreTouch",
                                     "-XX:+UseG1GC",
                                     "-XX:+ScavengeBeforeFullGC",
                                     "-XX:+DisableExplicitGC",
                                     $"-XX:MaxDirectMemorySize={MAX_OFF_HEAP}m" },
                PeerAssemblyLoadingMode = PeerAssemblyLoadingMode.CurrentAppDomain,
                DiscoverySpi = new Apache.Ignite.Core.Discovery.Tcp.TcpDiscoverySpi
                {

                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        Endpoints = new[] { $"{ClusterNode.Address}:{(ClusterNode.Port != 0 ? ClusterNode.Port : DEFAULT_PORT)}" }
                    },
                },
                Logger = new IgniteNLogLogger()
            };
            return cfg;
        }
    }
}