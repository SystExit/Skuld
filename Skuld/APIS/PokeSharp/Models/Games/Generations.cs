using PokeSharp.Models.Common;
using PokeSharp.Models.Games;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class Generations
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "abilities")]
        public NamedAPIResource[] Abilities { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "main_region")]
        public NamedAPIResource MainRegion { get; set; }
        [JsonProperty(PropertyName = "moves")]
        public NamedAPIResource[] Moves { get; set; }
        [JsonProperty(PropertyName = "pokemon_species")]
        public NamedAPIResource[] PokemonSpecies { get; set; }
        [JsonProperty(PropertyName = "types")]
        public NamedAPIResource[] Types { get; set; }
        [JsonProperty(PropertyName = "version_groups")]
        public NamedAPIResource[] VersionGroups { get; set; }
    }
}
