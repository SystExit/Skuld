using Newtonsoft.Json;

namespace Skuld.APIS.UrbanDictionary.Models
{
    public class UrbanWord
    {
        [JsonProperty("word")]
        public string Word { get; internal set; }

        [JsonProperty("definition")]
        public string Definition { get; internal set; }

        [JsonProperty("permalink")]
        public string PermaLink { get; internal set; }

        [JsonProperty("example")]
        public string Example { get; internal set; }

        [JsonProperty("author")]
        public string Author { get; internal set; }

        [JsonProperty("thumbs_up")]
        public string UpVotes { get; internal set; }

        [JsonProperty("thumbs_down")]
        public string DownVotes { get; internal set; }
    }
}