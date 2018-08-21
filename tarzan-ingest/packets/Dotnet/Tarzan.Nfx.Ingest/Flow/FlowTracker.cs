using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netdx.ConversationTracker;
using PacketDotNet;
using SharpPcap;

namespace Tarzan.Nfx.Ingest
{
    class FlowTracker
    {
        private ICaptureDevice m_device;
        private FlowTable m_table;
        private FlowIndex m_index;

        public FlowTracker(ICaptureDevice device)
        {
            this.m_device = device;
            m_table = new FlowTable();
            m_index = new FlowIndex(1000, 100, (int)m_device.LinkType);
        }

        public FlowIndex Index => m_index;
        public FlowTable Table => m_table;

        public void Track()
        {
            var tracker = new Tracker<(Packet, PosixTimeval), FlowKey, PacketStream>(m_table, m_table, m_table);
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
                m_index.Add(++packetCount, packetOffset, flowKey);
                packetOffset += e.Packet.Data.Length + 4 * sizeof(uint);
            }
            void Device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
            {
                captureTsc.SetResult(status);
            }

            m_device.Open();
            m_device.Capture();
            // following will block until capture is completed. 
            var result = captureTask.Result;
            m_device.Close();
        }
    }
}
