using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using Tarzan.Nfx.Ingest.Ignite;

namespace Tarzan.Nfx.Ingest
{
    partial class IgniteFlowTracker
    {
        public LinkLayers LinkLayers { get; }
        public int FlowCount { get; internal set; }

        private ICaptureDevice m_device;
        private IIgnite m_ignite;

        public IgniteFlowTracker(IIgnite ignite, string inputFile)
        {
            m_ignite = ignite;
            var fileinfo = new FileInfo(inputFile);
            var device = new CaptureFileReaderDevice(fileinfo.FullName);
            LinkLayers = device.LinkType;
            this.m_device = device;

        }


        public async Task TrackAsync()
        {
            var flowTable = new FlowTable();
            var tracker = new Tracker<(Packet, PosixTimeval), FlowKey, PacketStream>(flowTable, new KeyProvider(), new RecordProvider());
            var captureTsc = new TaskCompletionSource<CaptureStoppedEventStatus>();
            var captureTask = captureTsc.Task;
            var packetOffset = 6 * sizeof(uint);
            var packetCount = 0;
            m_device.OnPacketArrival += Device_OnPacketArrival;
            m_device.OnCaptureStopped += Device_OnCaptureStopped;
            void Device_OnPacketArrival(object sender, CaptureEventArgs e)
            {
                var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var flowRecord = tracker.UpdateFlow((packet, e.Packet.Timeval), out var flowKey);
                packetOffset += e.Packet.Data.Length + 4 * sizeof(uint);
                // log every 1000th packet
                if (packetCount % 1000 == 0) Console.Write('.');
            }
            void Device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
            {
                captureTsc.SetResult(status);
            }

            m_device.Open();
            m_device.Capture();
            await captureTask;
            m_device.Close();

            var globalTable = new FlowCache(m_ignite);

            /// THIS CODE DOES NOT WORK!!!
            using (var ldr = globalTable.GetDataStreamer())
            {
                // STREAMING: https://apacheignite-net.readme.io/docs/streaming
                foreach (var item in flowTable.Entries)
                {
                    await ldr.AddData(item.Key.ToString(), item.Value);
                    Console.Write(':');
                }
            }
        }
    }
}
