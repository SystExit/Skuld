using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonSpeciesGender
    {
        [JsonProperty(PropertyName = "rate")]
        public int? Rate { get; set; }
        [JsonProperty(PropertyName = "pokemon_species")]
        public NamedAPIResource PokemonSpecies { get; set; }
    }
}
