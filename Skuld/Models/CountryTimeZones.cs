using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.Models
{
    public class CountryTimeZones
    {
        [JsonProperty(PropertyName = "value")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "abbr")]
        public string Abbreviation { get; set; }
        [JsonProperty(PropertyName = "offset")]
        public double Offset { get; set; }
        [JsonProperty(PropertyName = "isdst")]
        public bool IsDst { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "utc")]
        public string[] UTC { get; set; }
    }
}
