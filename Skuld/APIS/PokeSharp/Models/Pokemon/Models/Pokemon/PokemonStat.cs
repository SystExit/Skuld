using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonStat
    {
        [JsonProperty(PropertyName = "stat")]
        public NamedAPIResource Stat { get; set; }
        [JsonProperty(PropertyName = "effort")]
        public int? Effort { get; set; }
        [JsonProperty(PropertyName = "base_stat")]
        public int? BaseStat { get; set; }
    }
}
