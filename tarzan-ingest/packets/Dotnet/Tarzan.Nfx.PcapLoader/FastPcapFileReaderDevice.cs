using PacketDotNet;
using SharpPcap;
using System;
using System.IO;
using System.Net.NetworkInformation;

namespace Tarzan.Nfx.PcapLoader
{
    class FastPcapFileReaderDevice : ICaptureDevice, IDisposable
    {
        readonly string m_filename;
        private BinaryReader m_reader;
        private LinkLayers m_network;

        public FastPcapFileReaderDevice(string filename)
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Close();
                    m_reader.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FastPcapFileReaderDevice() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
