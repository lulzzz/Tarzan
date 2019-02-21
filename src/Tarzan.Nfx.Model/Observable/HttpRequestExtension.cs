using Newtonsoft.Json;
using Tarzan.Nfx.Model.Core;

namespace Tarzan.Nfx.Model.Observable
{
    public class HttpRequestExtension : ObjectExtension
    {
        [JsonProperty("request_method")]
        public string RequestMethod { get; set; }

        [JsonProperty("request_value")]
        public string RequestValue { get; set; }

        [JsonProperty("request_version")]
        public string RequestVersion { get; set; }

        [JsonProperty("request_header")]
        public RequestHeader RequestHeader { get; set; }

        [JsonProperty("message_body_length")]
        public int MessageBodyLength { get; set; }

        [JsonProperty("message_body_data_ref")]
        public string MessageBodyDataRef { get; set; }
        public override string Type => "http-request-ext";
    }

    public class RequestHeader
    {
        public string AcceptEncoding { get; set; }
        public string UserAgent { get; set; }
        public string Host { get; set; }
    }

}
