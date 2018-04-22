using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonHeldItemVersion
    {
        [JsonProperty(PropertyName = "version")]
        public NamedAPIResource Version { get; set; }
        [JsonProperty(PropertyName = "rarity")]
        public int? Rarity { get; set; }
    }
}
