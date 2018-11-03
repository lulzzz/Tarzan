using Microsoft.Extensions.CommandLineUtils;
using NLog;
using NLog.Config;
using NLog.Targets;
using ShellProgressBar;
using System;
using System.IO;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Utils;

namespace Tarzan.Nfx.PcapLoader
{
    class Program
    {

        enum ProgramMode { Put, Stream, Verify }
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(true);
            var chunkSizeOption = commandLineApplication.Option("-ChunkSize", "A size of processing chunk. Packets are loaded and processed in chunks.", CommandOptionType.SingleValue);
            var clusterOption = commandLineApplication.Option("-Cluster", "Enpoint string of any cluster node.", CommandOptionType.SingleValue);
            var sourceFileOption = commandLineApplication.Option("-SourceFile", "Source pcap file to be loaded to the cluster.", CommandOptionType.MultipleValue);
            var sourceFolderOption = commandLineApplication.Option("-SourceFolder", "Folder where to read source pcap files to be loaded to the cluster.", CommandOptionType.MultipleValue);
            var modeOption = commandLineApplication.Option("-Mode", "Mode of loading data to Ignite cluster. It can be either 'put' or 'stream'. Mode verify is available to test data integrity.", CommandOptionType.SingleValue);
            var writeToOption = commandLineApplication.Option("-WriteTo", "Name of the cache where the loader stores the loaded frames. If not specified, the cache will have the same name of the name of the source file.", CommandOptionType.SingleValue);
            var disableProgressBarOption = commandLineApplication.Option("-DisableProgressBar", "Disables progress bar.", CommandOptionType.NoValue);
            commandLineApplication.OnExecute(async () =>
            {
                var mode = modeOption.HasValue() ? Enum.TryParse<ProgramMode>(modeOption.Value(), true, out var parsedMode) ? parsedMode : ProgramMode.Put : ProgramMode.Put;

                if (sourceFileOption.HasValue() || sourceFolderOption.HasValue())
                {
                    string verb = "";
                    IPcapProcessor loader = null;
                    switch (mode)
                    {
                        case ProgramMode.Put: loader = new PcapLoader(); verb = "Putting";  break;
                        case ProgramMode.Stream: loader = new PcapStreamer(); verb = "Streaming"; break;
                        case ProgramMode.Verify: loader = new PcapVerifier(); verb = "Verifying"; break;
                        default: loader = new PcapLoader(); break;
                    }
                    foreach (var file in sourceFileOption.Values)
                    {
                        if (File.Exists(file)) loader.SourceFiles.Add(new FileInfo(file));
                    }
                    foreach (var folder in sourceFolderOption.Values)
                    {
                        foreach (var file in Directory.EnumerateFiles(folder, "*.?cap"))
                        {
                            loader.SourceFiles.Add(new FileInfo(file));
                        }
                    }
                    if (chunkSizeOption.HasValue() && int.TryParse(chunkSizeOption.Value(), out var chunkSize)) { loader.ChunkSize = chunkSize; }

                    if (writeToOption.HasValue()) { loader.FrameCacheName = writeToOption.Value();  }

                    if (clusterOption.HasValue())
                    {
                        var ep = IPEndPointExtensions.Parse(clusterOption.Value(), 0);
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
                    if (disableProgressBarOption.HasValue())
                    {
                        SetupLogging(true);
                        await loader.Invoke();
                    }
                    else
                    {
                        SetupLogging(false);
                        Console.Clear();
                        using (var pbar = new ProgressBar(loader.SourceFiles.Count, "", pbRootOptions))
                        {
                            var fileNumber = 1;
                            var loadedBytes = 24;
                            var storedBytes = 24;
                            var errorPackets = 0;
                            var errorBytes = 24;
                            ChildProgressBar pbLoad = null;
                            ChildProgressBar pbStore = null;
                            ChildProgressBar pbError = null;
                            void Loader_OnFileOpen(object sender, FileInfo fileInfo)
                            {
                                pbar.Message = $"Processing file {fileNumber} of {loader.SourceFiles.Count}:";
                                pbStore = pbar.Spawn((int)fileInfo.Length, $"{verb} packets in Ignite cache:", pbChildOptions);
                                pbLoad = pbar.Spawn((int)fileInfo.Length, $"Loading packets from '{fileInfo.Name}', total size {fileInfo.Length} bytes:", pbChildOptions);
                                pbError = pbar.Spawn((int)fileInfo.Length, $"Error packets {errorPackets}:", pbChildOptions);
                            }
                            void Loader_OnFileCompleted(object sender, FileInfo fileInfo)
                            {
                                fileNumber++;
                                loadedBytes = 24;
                                storedBytes = 24;
                                pbar.Tick();
                                pbLoad.Tick(pbLoad.MaxTicks);
                                pbStore.Tick(pbStore.MaxTicks);
                                pbError.Tick(pbError.MaxTicks);
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
                            void Loader_OnErrorFrame(object sender, FileInfo fileInfo, int frameNumber, FrameData frame)
                            {
                                if (frame != null) errorBytes += frame.Data.Length + 32;
                                errorPackets++;
                                pbError.Tick(errorBytes);
                                pbError.Message = $"Error packets {errorPackets}:";
                            }

                            loader.OnFileOpen += Loader_OnFileOpen;
                            loader.OnFileCompleted += Loader_OnFileCompleted;
                            loader.OnChunkLoaded += Loader_OnChunkLoaded;
                            loader.OnChunkStored += Loader_OnChunkStored;
                            loader.OnErrorFrame += Loader_OnErrorFrame;
                            await loader.Invoke();
                        }
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
                commandLineApplication.ShowHelp();
            }
            catch (ArgumentException e)
            {
                commandLineApplication.Error.WriteLine($"ERROR: {e.Message}");
                commandLineApplication.ShowHelp();
            }
        }
        static void SetupLogging(bool logToConsole)
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();
            
            // Step 2. Create targets
            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = @"[${date:format=HH\:mm\:ss}] ${level}: ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);

            var fileTarget = new FileTarget("errorfile")
            {
                FileName = "${basedir}/Tarzan.Nfx.PcapLoader.err",
                Layout = "[${longdate}] ${level}: ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);

            config.AddRuleForOneLevel(LogLevel.Error, fileTarget);
            // Step 3. Define rules     
            if (logToConsole)       
                config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget); 

            // Step 4. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}
