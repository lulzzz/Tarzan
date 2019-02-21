using Apache.Ignite.Core;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.Ignite;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    internal class PcapVerifier : PcapProcessor
    {

        public override async Task Invoke()
        {
            var cfg = GetIgniteConfiguration();
            Ignition.ClientMode = true;
            using (var client = Ignition.Start(cfg))
            {
                foreach (var fileInfo in SourceFiles)
                {
                    OnFileOpened(fileInfo);
                    using (var device = new FastPcapFileReaderDevice(fileInfo.FullName))
                    {
                        await ProcessFile(client, fileInfo, device);
                    }
                    OnFileCompleted(fileInfo);
                }
            }
        }
        private async Task ProcessFile(IIgnite client, FileInfo fileInfo, FastPcapFileReaderDevice device)
        {
            device.Open();
            var frameKeyProvider = new FrameKeyProvider();
            var cache = CacheFactory.GetOrCreateFrameCache(client, FrameCacheName ?? fileInfo.Name);
            RawCapture rawCapture = null;
            var frameIndex = 0;
            var currentChunkBytes = 0;
            var currentChunkNumber = 0;
            while ((rawCapture = device.GetNextPacket()) != null)
            {
                currentChunkBytes += rawCapture.Data.Length + 4 * sizeof(int);

                var frame = new FrameData
                {
                    LinkLayer = (LinkLayerType)rawCapture.LinkLayerType,
                    Timestamp = rawCapture.Timeval.ToUnixTimeMilliseconds(),
                    Data = rawCapture.Data
                };
                var frameKey = new FrameKey(frameIndex, frameKeyProvider.GetKeyHash(frame));
                var storedFrame = await cache.GetAsync(frameKey);
                if (storedFrame == null) OnErrorFrameOccured(fileInfo, frameIndex, null);
                if (storedFrame != null && frame.Timestamp != storedFrame.Timestamp) OnErrorFrameOccured(fileInfo, frameIndex, storedFrame);

                if (frameIndex % ChunkSize == ChunkSize - 1)
                {
                    OnChunkLoaded(currentChunkNumber, currentChunkBytes);
                    OnChunkStored(currentChunkNumber, currentChunkBytes);
                    currentChunkNumber++;
                    currentChunkBytes = 0;
                }
                frameIndex++;
            }
            device.Close();
        }
    }
}