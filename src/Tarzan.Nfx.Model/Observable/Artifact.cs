namespace Tarzan.Nfx.Model.Observable
{
    using Newtonsoft.Json;
    using System;
    using Tarzan.Nfx.Model.Core;

    [Serializable]
    public partial class Artifact : ObservableObject
    {
        public override string Type => "artifact";

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("payload_bin", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] PayloadBin { get; set; }

        [JsonProperty("hashes")]
        public Hashes Hashes { get; set; }

        public static Artifact FromJson(string json) => JsonConvert.DeserializeObject<Artifact>(json, Converter.Settings);

        public string ToJson() => JsonConvert.SerializeObject(this, Converter.Settings);
    }      
}

