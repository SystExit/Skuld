using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonSpeciesVariety
    {
        [JsonProperty(PropertyName = "is_default")]
        public bool IsDefault { get; set; }
        [JsonProperty(PropertyName = "pokemon")]
        public NamedAPIResource Pokemon { get; set; }
    }
}
