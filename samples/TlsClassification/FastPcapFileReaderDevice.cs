using System;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.PacketDecoders;
namespace Tarzan.Nfx.Samples.TlsClassification
{
    class FastPcapFileReaderDevice : IDisposable
    {
        readonly string m_filename;
        private BinaryReader m_reader;
        private LinkLayerType m_network;

        public FastPcapFileReaderDevice(string filename)
        {
            m_filename = filename;
        }

        public string Name => m_filename;


        public static IEnumerable<FrameData> ReadAll(string filename)
        {
            using(var reader = new FastPcapFileReaderDevice(filename))
            {
                reader.Open();
                FrameData frame;
                while((frame = reader.GetNextPacket()) != null)
                {
                    yield return frame;
                }
                reader.Close();
            }
        }
        public FrameData GetNextPacket()
        {
            if (m_reader.BaseStream.Position + 16 <= m_reader.BaseStream.Length)
            {
                var tsSeconds = m_reader.ReadUInt32();
                var tsMicroseconds = m_reader.ReadUInt32();
                var timeval = new PosixTime(tsSeconds, tsMicroseconds);
                var includedLength = m_reader.ReadUInt32();
                var originalLength = m_reader.ReadUInt32();

                if ((m_reader.BaseStream.Position + includedLength) <= m_reader.BaseStream.Length)
                {
                    var frameBytes = new byte[includedLength];
                    m_reader.BaseStream.Read(frameBytes, 0, (int)includedLength);
                    return new FrameData { LinkLayer = m_network, Timestamp = timeval.ToUnixTimeMilliseconds(), Data = frameBytes};
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        public void Close()
        {
            m_reader.Close();
        }
        public void Open()
        {
            var stream = File.OpenRead(m_filename);
            m_reader = new BinaryReader(stream);
            ReadHeader();
        }


        void ReadHeader()
        {
            var magicNumber = m_reader.ReadUInt32();
            var version_major = m_reader.ReadUInt16();
            var version_minor = m_reader.ReadUInt16();
            var thiszone = m_reader.ReadInt32();
            var sigfigs = m_reader.ReadUInt32();
            var snaplen = m_reader.ReadUInt32();
            m_network = (LinkLayerType)m_reader.ReadUInt32();
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
