using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
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
    class PcapFileReaderDevice : ICaptureDevice
    {
        string m_filename;
        private BinaryReader m_reader;
        private LinkLayers m_network;

        public PcapFileReaderDevice(string filename)
        {
            m_filename = filename;
        }

        public string Name => m_filename;

        public string Description => throw new NotImplementedException();

        public string LastError => throw new NotImplementedException();

        public string Filter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICaptureStatistics Statistics => throw new NotImplementedException();

        public PhysicalAddress MacAddress => throw new NotImplementedException();

        public bool Started => throw new NotImplementedException();

        public TimeSpan StopCaptureTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public LinkLayers LinkType => throw new NotImplementedException();

        public event PacketArrivalEventHandler OnPacketArrival;
        public event CaptureStoppedEventHandler OnCaptureStopped;

        public RawCapture GetNextPacket()
        {
            if (m_reader.BaseStream.Position + 16 <= m_reader.BaseStream.Length)
            {
                var tsSeconds = m_reader.ReadUInt32();
                var tsMicroseconds = m_reader.ReadUInt32();
                var timeval = new PosixTimeval(tsSeconds, tsMicroseconds);
                var includedLength = m_reader.ReadUInt32();
                var originalLength = m_reader.ReadUInt32();

                if ((m_reader.BaseStream.Position + includedLength) <= m_reader.BaseStream.Length)
                {
                    var frameBytes = new byte[includedLength];
                    m_reader.BaseStream.Read(frameBytes, 0, (int)includedLength);
                    return new RawCapture(m_network, timeval, frameBytes);
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public void Capture()
        {
            while(true)
            {
                var capture = GetNextPacket();
                if (capture != null)
                {
                    OnPacketArrival?.Invoke(this, new CaptureEventArgs(capture, this));
                }
                else
                {
                    OnCaptureStopped?.Invoke(this, new CaptureStoppedEventStatus());
                    break;
                }
            }
        }

        public void Close()
        {
            m_reader.Close();
        }

        public int GetNextPacketPointers(ref IntPtr header, ref IntPtr data)
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            var stream = File.OpenRead(m_filename);
            m_reader = new BinaryReader(stream);
            ReadHeader();
        }

        public void Open(DeviceMode mode)
        {
            throw new NotImplementedException();
        }

        public void Open(DeviceMode mode, int read_timeout)
        {
            throw new NotImplementedException();
        }

        public void Open(DeviceMode mode, int read_timeout, MonitorMode monitor_mode)
        {
            throw new NotImplementedException();
        }

        public void SendPacket(Packet p)
        {
            throw new NotImplementedException();
        }

        public void SendPacket(Packet p, int size)
        {
            throw new NotImplementedException();
        }

        public void SendPacket(byte[] p)
        {
            throw new NotImplementedException();
        }

        public void SendPacket(byte[] p, int size)
        {
            throw new NotImplementedException();
        }

        public void StartCapture()
        {
            throw new NotImplementedException();
        }

        public void StopCapture()
        {
            throw new NotImplementedException();
        }

        void ReadHeader()
        {
            var magicNumber = m_reader.ReadUInt32();
            var version_major = m_reader.ReadUInt16();
            var version_minor = m_reader.ReadUInt16();
            var thiszone = m_reader.ReadInt32();
            var sigfigs = m_reader.ReadUInt32();
            var snaplen = m_reader.ReadUInt32();
            m_network = (LinkLayers)m_reader.ReadUInt32();
        }
    }

    partial class IgniteFlowTracker
    {
        public int FlowCount { get; internal set; }
        private ICaptureDevice m_device;
        private IIgnite m_ignite;

        /// <summary>
        /// Creates a new flow tracker instance. 
        /// 
        /// </summary>
        /// <param name="ignite">An ignite interface for performing cache operation.</param>
        /// <param name="inputFile">Name of the input file. The data will be read from this file.</param>
        public IgniteFlowTracker(IIgnite ignite, string inputFile)
        {
            m_ignite = ignite;
            m_device = new PcapFileReaderDevice(inputFile);
        }


        public async Task TrackAsync()
        {
            var flowTable = new FlowTable();
            var tracker = new Tracker<(Packet, PosixTimeval), FlowKey, PacketStream>(flowTable, new KeyProvider(), new RecordProvider());
            var packetCount = 0;
            m_device.Open();
            var sw = new Stopwatch();
            sw.Start();
            Console.Write("Processing Capture");
            while(true)
            {
                var capture = m_device.GetNextPacket();
                if (capture == null) break;
                //var packet = Packet.ParsePacket(capture.LinkLayerType, capture.Data);
                //var flowRecord = tracker.UpdateFlow((packet, capture.Timeval), out var flowKey);
                // log every time we process hundred thousands packets
                if ((++packetCount % 1000) == 0) Console.Write('.');
            }
            sw.Stop();
            Console.WriteLine($"Done ({sw.Elapsed}), packets={packetCount}, flows={flowTable.Count}.");
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
