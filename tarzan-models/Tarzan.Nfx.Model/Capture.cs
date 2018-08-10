using System;
using System.Net;
using Cassandra.Mapping;

namespace Tarzan.Nfx.Model
{

    /// <summary>
    /// Represents a single flow record.
    /// </summary>
    public class Capture
    {
        /// <summary>
        /// A unique identifier of the capture file.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The name of the capture file..
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A type of the capture file. It can be pcap, pcapng, etc.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The total size of the capture file.
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// Timestamp when the file was originally created.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// Timestamp when the file was upload to the system.
        /// </summary>
        /// <returns></returns>
        public DateTime UploadedOn { get; set; }
        /// <summary>
        /// Hash value computed by MD5 algorithm.
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// Name of the person who captured the file.
        /// </summary>
        public string Author { get; set;}

        /// <summary>
        /// Arbitrary notes associated with the capture.
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// List of tags that label the capture.
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Creates an empty flow record.
        /// </summary>
        public Capture()
        {

        }

    }
}