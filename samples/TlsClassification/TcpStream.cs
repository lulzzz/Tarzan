using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PacketDotNet;

namespace Tarzan.Nfx.Samples.TlsClassification
{

    /// <summary>
    /// Represents some metainformation about a generic packet.
    /// </summary>
    public struct PacketMeta
    {
        /// <summary>
        /// A number of the packet in the collection.
        /// </summary>
        public int Number;
        /// <summary>
        /// A unix based timestamp of the packet.
        /// </summary>
        public long Timestamp;

        public override string ToString()
        {
            return $"PacketMeta: Number={Number}, Timestamp={Timestamp}";
        }
    }

    public class TcpStream<TSegment> : Stream
    {
        IList<TSegment> m_packets;
        int m_currentPacket = 0; 
        int m_offsetInPacketPayload = 0;
        int m_absolutePosition;
        int? m_length;

        public TcpStream(Func<TSegment,byte[]> getPayload, IEnumerable<TSegment> tcpPackets)
        {
            m_packets = tcpPackets.ToList();
            this.getPayload = getPayload;
        }


        /// <summary>
        /// Gets the tcp packets that contains data from he given offset and of the specified amount.
        /// It just computes which TcpSegments overlaps with the given range.
        /// </summary>
        /// <returns>The tcp packets.</returns>
        /// <param name="offset">Offset.</param>
        /// <param name="length">Length.</param>
        public IEnumerable<(TSegment Segment, Range<long> Range)> GetSegments(long offset, int length)
        {
            // TODO: improve the code by not enumerate all the packets!
            var currentOffset = 0;
            var givenRange = new Range<long>(offset, offset + (length -1));
            foreach (var tcp in m_packets)
            {
                var tcpPayloadLen = getPayload(tcp).Length;
                var packetRange = new Range<long>(currentOffset, currentOffset + (tcpPayloadLen-1));


                if (givenRange.IsOverlapped(packetRange))
                {
                    var range = givenRange.Intersect(packetRange).Shift(x => x-currentOffset);
                    yield return (tcp, range);
                }
                currentOffset += tcpPayloadLen;
            }
        }
        public TSegment CurrentPacket => m_packets[m_currentPacket];

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                if (m_length == null)
                {
                    m_length = m_packets.Sum(x => getPayload(x).Length);
                }
                return m_length.Value;
            }
        }
        public override long Position { get => m_absolutePosition; set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remainingCount = count;
            var targetBuffer = new Span<byte>(buffer, offset, remainingCount);
            var readCount = 0;

            while (m_currentPacket < m_packets.Count)
            {
                var currentBuffer = new Span<byte>(getPayload(m_packets[m_currentPacket])).Slice(m_offsetInPacketPayload);

                if (currentBuffer.Length == 0)
                {
                    m_currentPacket++;
                    continue;
                }
                if (currentBuffer.Length > remainingCount)
                {
                    currentBuffer.Slice(0, remainingCount).CopyTo(targetBuffer);
                    m_offsetInPacketPayload += remainingCount;
                    m_absolutePosition += remainingCount;
                    readCount += remainingCount;
                    remainingCount = 0;
                    break;
                }
                if (currentBuffer.Length == remainingCount)
                {
                    currentBuffer.CopyTo(targetBuffer);
                    m_offsetInPacketPayload = 0;
                    m_currentPacket++;
                    m_absolutePosition += remainingCount;
                    readCount += remainingCount;
                    remainingCount = 0;
                    break;
                }
                if (currentBuffer.Length < remainingCount)
                {
                    // take the rest and move to the next packet...
                    currentBuffer.CopyTo(targetBuffer);
                    targetBuffer = targetBuffer.Slice(currentBuffer.Length);
                    m_currentPacket++;
                    m_offsetInPacketPayload = 0;
                    m_absolutePosition += currentBuffer.Length;
                    readCount += currentBuffer.Length;
                    remainingCount -= currentBuffer.Length;
                    continue;
                }
            }
            return readCount;
        }
        internal static readonly byte[] EmptyBuffer = new byte[0];
        private readonly Func<TSegment, byte[]> getPayload;

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    public class Range<T> where T : IComparable
    {
        readonly T min;
        readonly T max;

        public Range(T min, T max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsOverlapped(Range<T> other)
        {
            return Min.CompareTo(other.Max) <= 0 && other.Min.CompareTo(Max) <= 0;
        }

        internal Range<T> Intersect(Range<T> other)
        {
            if (IsOverlapped(other))
            {
                var newMin = Min.CompareTo(other.Min) > 0 ? Min : other.Min;
                var newMax = Max.CompareTo(other.Max) < 0 ? Max : other.Max;
                return new Range<T>(newMin, newMax);
            }
            else
                return null;
        }

        public Range<T> Shift(Func<T,T> shiftFunc)
        {
            var newMin = shiftFunc(min);
            var newMax = shiftFunc(max);
            return new Range<T>(newMin, newMax);
        }


        public T Min { get { return min; } }
        public T Max { get { return max; } }


        public override string ToString()
        {
            return $"[{Min}-{Max}]";
        }
    }
}
