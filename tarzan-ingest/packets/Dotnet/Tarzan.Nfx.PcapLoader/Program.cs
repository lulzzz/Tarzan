using Apache.Ignite.Core;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Client.Cache;
using Apache.Ignite.Core.Communication.Tcp;
using Microsoft.Extensions.CommandLineUtils;
using SharpPcap;
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
                    ExecuteLoader(clusterArgument.Values, fileArgument.Values, folderArgument.Values);
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

        private static void ExecuteLoader(List<string> clusterEp, List<string> files, List<string> folders)
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

            const int CACHE_SIZE = 1000;
            using (var client = Ignition.StartClient(cfg))
            {
                foreach (var fileInfo in pcpafiles)
                {
                    Console.WriteLine($"Processing file '{fileInfo.Name}', total size {fileInfo.Length}.");
                    var sw = new Stopwatch();

                    using (var device = new FastPcapFileReaderDevice(fileInfo.FullName))
                    {
                        var packetCache = client.GetOrCreateCache<int, RawCapture>(fileInfo.FullName);
                        Console.WriteLine($"Packets already in cache {packetCache.GetSize()}.");
                        sw.Start();
                        device.Open();
                        
                        int frameIndex = 0;
                        var frameArray = new KeyValuePair<int, RawCapture>[CACHE_SIZE];
                        var putCapturesToCacheTask = Task.CompletedTask;

                        RawCapture rawCapture = null;
                        while ((rawCapture = device.GetNextPacket()) != null)
                        {
                            frameArray[frameIndex % CACHE_SIZE] = KeyValuePair.Create(frameIndex, rawCapture);
                            if (frameIndex % CACHE_SIZE == CACHE_SIZE-1)
                            {
                                
                                var cacheToStore = frameArray;
                                frameArray = new KeyValuePair<int, RawCapture>[CACHE_SIZE];
                                putCapturesToCacheTask = putCapturesToCacheTask.ContinueWith((_) => { Console.Write("s"); return packetCache.PutAllAsync(cacheToStore); });
                                Console.Write("p");
                            }
                            frameIndex++;
                        }
                        putCapturesToCacheTask = putCapturesToCacheTask.ContinueWith((_) => { Console.Write("s"); return packetCache.PutAllAsync(frameArray.Take(frameIndex % CACHE_SIZE)); });

                        putCapturesToCacheTask.Wait();
                        sw.Stop();
                        Console.WriteLine($"Read packets: {frameIndex}, packets in cache {packetCache.GetSize()}, total time elapsed: {sw.Elapsed}.");
                        device.Close();
                    }
                }
            }
        }
    }
}
