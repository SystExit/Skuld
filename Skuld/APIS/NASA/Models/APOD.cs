using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.NASA.Models
{
    public class APOD
    {
        [JsonProperty(PropertyName = "copyright")]
        public string CopyRight { get; internal set; }
        [JsonProperty(PropertyName = "date")]
        public string Date { get; internal set; }
        [JsonProperty(PropertyName = "explanation")]
        public string Explanation { get; internal set; }
        [JsonProperty(PropertyName = "hdurl")]
        public string HDUrl { get; internal set; }
        [JsonProperty(PropertyName = "media_type")]
        public string MediaType { get; internal set; }
        [JsonProperty(PropertyName = "service_version")]
        public string ServiceVersion { get; internal set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; internal set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; internal set; }
    }
}
