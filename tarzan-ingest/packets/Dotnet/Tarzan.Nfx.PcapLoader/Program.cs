using Apache.Ignite.Core;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Client.Cache;
using Apache.Ignite.Core.Communication.Tcp;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging.Console;
using SharpPcap;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tarzan.Nfx.FlowTracker;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);

            var clusterArgument = commandLineApplication.Option("-c|--cluster", "Enpoint string of any cluster node.", CommandOptionType.MultipleValue);
            var fileArgument = commandLineApplication.Option("-f|--file", "Source pcap file(s) to be loaded to the cluster.", CommandOptionType.MultipleValue);
            var folderArgument = commandLineApplication.Option("-g|--folder", "Folder where to read source pcap files to be loaded to the cluster.", CommandOptionType.MultipleValue);

            commandLineApplication.OnExecute(() =>
            {
                if (clusterArgument.HasValue() &&
                (fileArgument.HasValue() || folderArgument.HasValue()))
                {
                    new Program().ExecuteLoader(clusterArgument.Values, fileArgument.Values, folderArgument.Values);
                    return 0;
                }
                else
                {
                    commandLineApplication.Error.WriteLine();
                    commandLineApplication.ShowHelp();
                    return -1;
                }
            });
            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
            }
            catch (ArgumentException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
            }
        }

        ConsoleLogger m_logger = new ConsoleLogger("Loader", (s, ll) => true, true);

        private void ExecuteLoader(List<string> clusterEp, List<string> files, List<string> folders)
        {
            var ep = IPEndPointExtensions.Parse(clusterEp.First(), 10800);
            var cfg = new IgniteClientConfiguration
            {
                Host = ep.Address.ToString(),
                Port = ep.Port 
            };
            var pcpafiles = new List<FileInfo>();
            foreach(var file in files)
            {
                if (File.Exists(file)) pcpafiles.Add(new FileInfo(file));
            }
            foreach(var folder in folders)
            {
                foreach(var file in Directory.EnumerateFiles(folder, "*.?cap"))
                {
                    pcpafiles.Add(new FileInfo(file));
                }
            }



            var pbRootOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ProgressCharacter = '─',
               
            };
            var pbChildOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.DarkGreen,
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            var flowKeyProvider = new FrameKeyProvider();
            const int CACHE_SIZE = 1000;
            using (var client = Ignition.StartClient(cfg))
            {
                using (var pbar = new ProgressBar(pcpafiles.Count, "", pbRootOptions))
                {
                    foreach (var fileInfo in pcpafiles)
                    {
                        pbar.Message = $"Processing file '{fileInfo.Name}', total size {fileInfo.Length} bytes. ";
                        var sw = new Stopwatch();

                        using (var device = new FastPcapFileReaderDevice(fileInfo.FullName))
                        using (var pbStore = pbar.Spawn((int)fileInfo.Length, $"Storing packets in Ignite cache...", pbChildOptions))
                        using (var pbLoad = pbar.Spawn((int)fileInfo.Length, $"Loading packets from pcap file...", pbChildOptions))
                        {
                            var packetCache = client.GetOrCreateCache<int, Frame>(fileInfo.Name);
                            m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "PcapLoader", 1, $"Packets already in cache {packetCache.GetSize()}.", null);

                            sw.Start();
                            device.Open();

                            int frameIndex = 0;
                            int estimatedDataOffset = 0;
                            var frameArray = new KeyValuePair<int, Frame>[CACHE_SIZE];
                            var putCapturesToCacheTask = Task.CompletedTask;

                            RawCapture rawCapture = null;
                            while ((rawCapture = device.GetNextPacket()) != null)
                            {
                                estimatedDataOffset += rawCapture.Data.Length + 8;
                                pbLoad.Tick(estimatedDataOffset);
                                var frame = new Frame
                                {
                                    LinkLayer = (LinkLayerType)rawCapture.LinkLayerType,
                                    Timestamp = rawCapture.Timeval.ToUnixTimeMilliseconds(),
                                    Data = rawCapture.Data
                                };

                                var key = flowKeyProvider.GetKey(frame);
                                var frameHashCode = key.HashCode;

                                frameArray[frameIndex % CACHE_SIZE] = KeyValuePair.Create(frameIndex, frame);
                                if (frameIndex % CACHE_SIZE == CACHE_SIZE - 1)
                                {

                                    var cacheToStore = frameArray;
                                    frameArray = new KeyValuePair<int, Frame>[CACHE_SIZE];
                                    putCapturesToCacheTask = putCapturesToCacheTask.ContinueWith(async (_) => { await packetCache.PutAllAsync(cacheToStore); pbStore.Tick(estimatedDataOffset); });
                                }
                                frameIndex++;
                            }

                            pbLoad.Tick((int)fileInfo.Length);
                            putCapturesToCacheTask = putCapturesToCacheTask.ContinueWith(async (_) => { await packetCache.PutAllAsync(frameArray.Take(frameIndex % CACHE_SIZE)); pbStore.Tick(estimatedDataOffset); });

                            // wait till we store all data to cache:
                            putCapturesToCacheTask.Wait();

                            pbStore.Tick((int)fileInfo.Length);
                            pbar.Tick();
                            sw.Stop();

                            m_logger.WriteMessage(Microsoft.Extensions.Logging.LogLevel.Information, "PcapLoader", 1, $"Read packets: {frameIndex}, packets in cache {packetCache.GetSize()}, total time elapsed: {sw.Elapsed}.", null);

                            device.Close();
                        }
                    }
                }
            }
        }
    }
}
