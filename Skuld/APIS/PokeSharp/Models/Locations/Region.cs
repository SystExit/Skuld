using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Locations
{
    public class Region
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "locations")]
        public NamedAPIResource[] Locations { get; set; }
        [JsonProperty(PropertyName = "main_generation")]
        public NamedAPIResource MainGeneration { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "pokedexes")]
        public NamedAPIResource[] Pokedexes { get; set; }
        [JsonProperty(PropertyName = "version_groups")]
        public NamedAPIResource[] VersionGroups { get; set; }
    }
}
