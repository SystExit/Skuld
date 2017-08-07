using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Berries
{
    public class BerryFlavors
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "berries")]
        public FlavorBerryMap[] Berries { get; set; }
        [JsonProperty(PropertyName = "contest_type")]
        public NamedAPIResource ContestType { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
    }
}
