using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class AbilityPokemon
    {
        [JsonProperty(PropertyName = "is_hidden")]
        public bool IsHidden { get; set; }
        [JsonProperty(PropertyName = "slot")]
        public int? Slot { get; set; }
        [JsonProperty(PropertyName = "pokemon")]
        public NamedAPIResource Pokemon { get; set; }
    }
}
