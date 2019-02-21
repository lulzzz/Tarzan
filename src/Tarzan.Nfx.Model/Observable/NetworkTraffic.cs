using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Tarzan.Nfx.Model.Core;

namespace Tarzan.Nfx.Model.Observable
{
    [Serializable]
    public partial class NetworkTraffic : ObservableObject
    {
        public override string Type => "network-traffic";

        [JsonProperty("src_ref")]
        public string SrcRef { get; set; }

        [JsonProperty("dst_ref")]
        public string DstRef { get; set; }

        [JsonProperty("src_port")]
        public int SrcPort { get; set; }

        [JsonProperty("dst_port")]
        public int DstPort { get; set; }

        [JsonProperty("protocols")]
        public string[] Protocols { get; set; }

        [JsonProperty("start")]
        public DateTimeOffset Start { get; set; }

        [JsonProperty("end")]
        public DateTimeOffset End { get; set; }

        [JsonProperty("src_packets")]
        public int SrcPackets { get; set; }

        [JsonProperty("dst_packets")]
        public int DstPackets { get; set; }

        [JsonProperty("src_byte_count")]
        public int SrcByteCount { get; set; }

        [JsonProperty("dst_byte_count")]
        public int DstByteCount { get; set; }

        [JsonProperty("src_payload_ref")]
        public string SrcPayloadRef { get; set; }

        [JsonProperty("dst_payload_ref")]
        public string DstPayloadRef { get; set; }

        [JsonProperty("ipfix")]
        public Ipfix Ipfix { get; set; }

        [JsonProperty("encapsulates_refs")]
        public string[] EncapsulatesRefs { get; set; }

        [JsonProperty("encapsulated_by_ref")]
        public string EncapsulatedByRef { get; set; }


        public string ToJson() => JsonConvert.SerializeObject(this, Converter.Settings);
        public static NetworkTraffic FromJson(string json) => JsonConvert.DeserializeObject<NetworkTraffic>(json, Converter.Settings);
    }

    public partial class Ipfix
    {
        [JsonProperty("minimum_ip_total_length")]
        public int MinimumIpTotalLength { get; set; }
        [JsonProperty("maximum_ip_total_length")]
        public int MaximumIpTotalLength { get; set; }
    }
}