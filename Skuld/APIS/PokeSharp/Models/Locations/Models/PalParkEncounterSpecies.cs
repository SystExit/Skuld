using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Locations
{
    public class PalParkEncounterSpecies
    {
        [JsonProperty(PropertyName = "base_score")]
        public int? BaseScore { get; set; }
        [JsonProperty(PropertyName = "rate")]
        public int? Rate { get; set; }
        [JsonProperty(PropertyName = "pokemon_species")]
        public NamedAPIResource PokemonSpecies { get; set; }
    }
}
