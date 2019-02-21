using Newtonsoft.Json;
using Tarzan.Nfx.Model.Core;

namespace Tarzan.Nfx.Model.Observable
{
    public class IcmpExtension : ObjectExtension
    {
        [JsonProperty("icmp_type_hex")]
        public string IcmpTypeHex { get; set; }

        [JsonProperty("icmp_code_hex")]
        public string IcmpCodeHex { get; set; }

        public override string Type => "icmp-ext";
    }
}