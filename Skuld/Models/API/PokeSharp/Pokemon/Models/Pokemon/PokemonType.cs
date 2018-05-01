using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonType
    {
        [JsonProperty(PropertyName = "slot")]
        public int? Slot { get; set; }
        [JsonProperty(PropertyName = "type")]
        public NamedAPIResource Type { get; set; }
    }
}
