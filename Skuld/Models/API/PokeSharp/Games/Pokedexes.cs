using PokeSharp.Models.Common;
using PokeSharp.Models.Games;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class Pokedexes
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "is_main_series")]
        public bool IsMainSeries { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public ListDescription[] Descriptions { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName Names { get; set; }
        [JsonProperty(PropertyName = "pokemon_entries")]
        public PokemonEntry[] PokemonEntries { get; set; }
        [JsonProperty(PropertyName = "region")]
        public NamedAPIResource Region { get; set; }
        [JsonProperty(PropertyName = "version_groups")]
        public NamedAPIResource[] VersionGroups { get; set; }
    }
}