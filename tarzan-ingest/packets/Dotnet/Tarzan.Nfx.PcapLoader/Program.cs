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
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    class Program
    {
        enum ProgramMode { Put, Stream }
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);

            var chunkSizeArgument = commandLineApplication.Option("-s|--chunk", "A size of processing chunk. Packets are loaded and processed in chunks.", CommandOptionType.SingleValue);
            var clusterArgument = commandLineApplication.Option("-c|--cluster", "Enpoint string of any cluster node.", CommandOptionType.SingleValue);
            var fileArgument = commandLineApplication.Option("-f|--file", "Source pcap file(s) to be loaded to the cluster.", CommandOptionType.MultipleValue);
            var folderArgument = commandLineApplication.Option("-g|--folder", "Folder where to read source pcap files to be loaded to the cluster.", CommandOptionType.MultipleValue);
            var modeArgument = commandLineApplication.Option("-m|--mode", "Mode of loading data to cache. It can be either 'put' or 'stream'.", CommandOptionType.SingleValue);
            commandLineApplication.OnExecute(async () =>
            {
                var mode = modeArgument.HasValue() ? Enum.TryParse<ProgramMode>(modeArgument.Value(), true, out var parsedMode) ? parsedMode : ProgramMode.Put : ProgramMode.Put;

                if (fileArgument.HasValue() || folderArgument.HasValue())
                {

                    var loader = mode == ProgramMode.Put ? new PcapLoader() as IPcapProcessor : new PcapStreamer() as IPcapProcessor;
                    foreach (var file in fileArgument.Values)
                    {
                        if (File.Exists(file)) loader.SourceFiles.Add(new FileInfo(file));
                    }
                    foreach (var folder in folderArgument.Values)
                    {
                        foreach (var file in Directory.EnumerateFiles(folder, "*.?cap"))
                        {
                            loader.SourceFiles.Add(new FileInfo(file));
                        }
                    }
                    if (chunkSizeArgument.HasValue() && int.TryParse(chunkSizeArgument.Value(), out var chunkSize)) { loader.ChunkSize = chunkSize; }

                    if (clusterArgument.HasValue())
                    {
                        var ep = IPEndPointExtensions.Parse(clusterArgument.Value(), 0);
                        loader.ClusterNode = ep;
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
                    using (var pbar = new ProgressBar(loader.SourceFiles.Count, "", pbRootOptions))
                    {
                        var fileNumber = 1;
                        var loadedBytes = 24;
                        var storedBytes = 24;
                        ChildProgressBar pbLoad = null; ChildProgressBar pbStore = null;

                        void Loader_OnFileOpen(object sender, FileInfo fileInfo)
                        {
                            pbar.Message = $"Processing file {fileNumber} of {loader.SourceFiles.Count}:";
                            pbStore = pbar.Spawn((int)fileInfo.Length, $"Storing packets in Ignite cache:", pbChildOptions);
                            pbLoad = pbar.Spawn((int)fileInfo.Length, $"Loading packets from '{fileInfo.Name}', total size {fileInfo.Length} bytes:", pbChildOptions);
                        }
                        void Loader_OnFileCompleted(object sender, FileInfo fileInfo)
                        {
                            fileNumber++;
                            loadedBytes = 24;
                            storedBytes = 24;
                        }
                        void Loader_OnChunkLoaded(object sender, int chunkNumber, int chunkBytes)
                        {
                            loadedBytes += chunkBytes;
                            pbLoad.Tick(loadedBytes);
                        }
                        void Loader_OnChunkStored(object sender, int chunkNumber, int chunkBytes)
                        {
                            storedBytes += chunkBytes;
                            pbStore.Tick(storedBytes);
                        }

                        loader.OnFileOpen += Loader_OnFileOpen;
                        loader.OnFileCompleted += Loader_OnFileCompleted; 
                        loader.OnChunkLoaded += Loader_OnChunkLoaded;
                        loader.OnChunkStored += Loader_OnChunkStored;

                        await loader.Invoke();
                    }
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
    }
}
