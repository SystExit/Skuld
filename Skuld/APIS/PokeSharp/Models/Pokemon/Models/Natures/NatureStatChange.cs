using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class NatureStatChange
    {
        [JsonProperty(PropertyName = "max_change")]
        public int? MaxChange { get; set; }
        [JsonProperty(PropertyName = "pokeathlon_stat")]
        public NamedAPIResource PokeathlonStat { get; set; }
    }
}
