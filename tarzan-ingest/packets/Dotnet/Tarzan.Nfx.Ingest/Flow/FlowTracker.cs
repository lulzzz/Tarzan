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
    public interface ICaptureProvider
    {
        Frame GetNextFrame();
    }

    class CaptureDeviceProvider : ICaptureProvider
    {

        ICaptureDevice m_device;

        public CaptureDeviceProvider(ICaptureDevice m_device)
        {
            this.m_device = m_device;
        }

        public Frame GetNextFrame()
        {
            var packet = m_device.GetNextPacket();
            if (packet != null)
            {
                return new Frame((LinkLayerType)packet.LinkLayerType, PosixTime.FromUnixTimeMilliseconds(packet.Timeval.ToUnixTimeMilliseconds()), packet.Data);
            }
            else return null;
        }
    }

    class FlowTracker
    {
        Dictionary<PacketFlowKey, PacketStream> m_flowTable;
        private ICaptureProvider m_capture;
        private int m_totalFrameCount;
        public FlowTracker(ICaptureProvider device)
        {
            this.m_capture = device;
            m_flowTable = new Dictionary<PacketFlowKey, PacketStream>();
           
        }
        public Dictionary<PacketFlowKey, PacketStream> FlowTable { get => m_flowTable; set => m_flowTable = value; }
        public int TotalFrameCount { get => m_totalFrameCount; }

        /// <summary>
        /// Captures all packets and tracks flows.
        /// </summary>
        public void CaptureAll()
        { 
            Frame frame = null;
            while((frame = m_capture.GetNextFrame()) != null)
            {
                m_totalFrameCount ++;
                var key = PacketFlowKey.GetKey(frame.Data);
                if (m_flowTable.TryGetValue(key, out var lst))
                {
                    PacketStream.Update(lst, frame);
                }
                else
                {
                    m_flowTable[key] = PacketStream.From(frame);
                }
            }
        }
    }
}
