using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonAbility
    {
        [JsonProperty(PropertyName = "is_hidden")]
        public bool IsHidden { get; set; }
        [JsonProperty(PropertyName = "slot")]
        public int? Slot { get; set; }
        [JsonProperty(PropertyName = "ability")]
        public NamedAPIResource Ability { get; set; }
    }
}
