using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.ResourceLists
{
    public class APIResourceList
    {
        [JsonProperty(PropertyName = "count")]
        public int? Count { get; set; }
        [JsonProperty(PropertyName = "next")]
        public string Next { get; set; }
        [JsonProperty(PropertyName = "previous")]
        public string Previous { get; set; }
        [JsonProperty(PropertyName = "results")]
        public APIResource[] Results { get; set; }
    }
}
