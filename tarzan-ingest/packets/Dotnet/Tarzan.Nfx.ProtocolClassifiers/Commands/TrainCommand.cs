using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;
using SharpPcap;
using SharpPcap.LibPcap;
using Tarzan.Nfx.Analyzers;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;

namespace Tarzan.Nfx.ProtocolClassifiers.Commands
{
    class TrainCommand
    {
        public string Name => "train-classifier";

        internal void ExecuteCommand(CommandLineApplication target)
        {
            target.Description = "Train all implemented classifiers using the pcaps in the specified folder. ";
            var inputFolder = target.Option("-folder", "Read packet data from source PCAP files in the specified folder.", CommandOptionType.SingleValue);


            target.OnExecute(() =>
            {
                if (inputFolder.HasValue())
                {
                    return Process(inputFolder.Value());
                }
                else
                {
                    throw new ArgumentException("Input folder with training pcaps has to be specified!");
                }
            });
        }

        private int Process(string trainingFolder)
        {
            // training folder contains pcaps for each protocol. Each pcap has name corresponding to the protocol. 
            var statClassifier = new Statistical.StatisticalClassifier();
            var portClassifier = new PortBased.PortBasedClassifier<FlowRecord<Statistical.FlowStatisticalVector>>();
            portClassifier.LoadConfiguration(null);
            foreach(var capFile in Directory.EnumerateFiles(trainingFolder,"*.cap"))
                using (var errorLog = new StreamWriter(File.Create(Path.ChangeExtension(capFile, "errors"))))
                {
                    var protocolName = Path.GetFileNameWithoutExtension(capFile).ToLowerInvariant();
                    // read pcap, compute flows, merge to biflows, and train the classifier:
                    var device = new CaptureFileReaderDevice(capFile);
                    // Open the device for capturing
                    var flowTracker = new FlowTracker(new FrameKeyProvider());
                    device.Open();

                    RawCapture packet = null;
                    var frameIndex = 0;
                    while ((packet = device.GetNextPacket()) != null)
                    {
                        frameIndex++;
                        try
                        {
                            var frame = new Frame
                            {
                                LinkLayer = (LinkLayerType)packet.LinkLayerType,
                                Timestamp = ToUnixTimeMilliseconds(packet.Timeval),
                                Data = packet.Data
                            };
                            flowTracker.ProcessFrame(frame);
                        }
                        catch (Exception e)
                        {
                            errorLog.WriteLine($"[#{frameIndex}] {e.Message}");
                        }
                    }
                    var flowTable = flowTracker.FlowTable.Select(f => KeyValuePair.Create(f.Key, new FlowRecord<Statistical.FlowStatisticalVector> { Flow = f.Value }));
                    Conversation<FlowRecord<Statistical.FlowStatisticalVector>> getConversation(KeyValuePair<FlowKey, FlowRecord<Statistical.FlowStatisticalVector>> upflow)
                    {
                        var downflow = flowTracker.FlowTable.GetValueOrDefault(upflow.Key.SwapEndpoints());
                        return new Conversation<FlowRecord<Statistical.FlowStatisticalVector>>
                        {
                            ConversationKey = upflow.Key,
                            Upflow = upflow.Value,
                            Downflow = new FlowRecord<Statistical.FlowStatisticalVector> { Flow = downflow }
                        };
                    }

                    // HACK: What if there is a single flow conversation, where src.port < dst.port ?
                    var conversations = flowTable.Where(f => f.Key.SourcePort > f.Key.DestinationPort).Select(f => getConversation(f)).ToList();

                    // TRAIN CLASSIFIERS FOR THE RECOGNIZED CONVERSATIONS:
                    foreach (var conversation in conversations)
                    {   
                        // use port based classifier to get rough information on the flow in the pcap:
                        var protocol = portClassifier.Match(conversation);
                        Console.WriteLine($"{conversation.ConversationKey.ToString()} | {protocol.ProtocolName} ({protocol.Similarity})");

                        statClassifier.Train(protocolName, conversation);
                    }


                device.Close();
            }
            return 0;
        }

        public static long ToUnixTimeMilliseconds(PosixTimeval timeval)
        {
            return (long)((timeval.Seconds * 1000) + (timeval.MicroSeconds / 1000));
        }
    }
}
