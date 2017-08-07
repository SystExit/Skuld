using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Utility
{
    public class Languages
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "official")]
        public bool Official { get; set; }
        [JsonProperty(PropertyName = "iso639")]
        public string ISO639 { get; set; }
        [JsonProperty(PropertyName = "iso3166")]
        public string ISO3166 { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
    }
}
