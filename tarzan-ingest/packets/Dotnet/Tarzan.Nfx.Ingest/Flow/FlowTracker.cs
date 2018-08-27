using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netdx.ConversationTracker;
using Netdx.PacketDecoders;
using PacketDotNet;
using SharpPcap;

namespace Tarzan.Nfx.Ingest
{
    class FlowTracker
    {
        Dictionary<PacketFlowKey, PacketStream> m_flowTable;
        private ICaptureDevice m_device;
        private FlowIndex m_index;

        public FlowTracker(ICaptureDevice device)
        {
            this.m_device = device;
            m_index = new FlowIndex(1000, 100, (int)m_device.LinkType);
            m_flowTable = new Dictionary<PacketFlowKey, PacketStream>();
        }

        public FlowIndex Index => m_index;

        public async Task TrackAsync()
        {
            var tracker = new Tracker<(Packet, PosixTimeval), FlowKey, PacketStream>(m_table, new KeyProvider(), new RecordProvider());
            var captureTsc = new TaskCompletionSource<CaptureStoppedEventStatus>();
            var captureTask = captureTsc.Task;
            var packetOffset = 6 * sizeof(uint);
            m_device.OnPacketArrival += Device_OnPacketArrival;
            m_device.OnCaptureStopped += Device_OnCaptureStopped;
            void Device_OnPacketArrival(object sender, CaptureEventArgs e)
            {
                var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var flowRecord = tracker.UpdateFlow((packet, e.Packet.Timeval), out var flowKey);
                //m_index.Add(++packetCount, packetOffset, flowKey);
                packetOffset += e.Packet.Data.Length + 4 * sizeof(uint);
            }
            void Device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
            {
                captureTsc.SetResult(status);
            }

            m_device.Open();
            RawCapture packet = null;
            while((packet = m_device.GetNextPacket()) != null)
            { 
                var key = PacketFlowKey.GetKey(packet.Data);
                if (m_flowTable.TryGetValue(key, out var lst))
                {
                    lst.PacketList.Add(packet);
                }
                else
                {
                    m_flowTable[key] = new List<RawCapture> { packet };
                }
            }
            m_device.Close();
        }
    }
}
