using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Items
{
    public class ItemPocket
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "categories")]
        public NamedAPIResource[] Categories { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
    }
}
