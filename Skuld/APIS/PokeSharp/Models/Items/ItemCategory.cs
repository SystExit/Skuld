using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Items
{
    public class ItemCategory
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "items")]
        public NamedAPIResource[] Items { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "pocket")]
        public NamedAPIResource Pocket { get; set; }
    }
}
