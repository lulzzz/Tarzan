using Newtonsoft.Json;

namespace Tarzan.Nfx.Model.Observable
{
    public class TcpExtension
    {
        /// <summary>
        /// Specifies the source TCP flags, as the union of all TCP flags observed between the start of the traffic  and the end of the traffic
        /// </summary>
        [JsonProperty("src_flags_hex")]
        public string SrcFlagsHex { get; set; }
        /// <summary>
        /// Specifies the destination TCP flags, as the union of all TCP flags observed between the start of the traffic and the end of the traffic.
        /// </summary>
        [JsonProperty("dst_flags_hex")]
        public string DstFlagsHex { get; set; }
    }

}
