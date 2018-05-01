using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonHeldItem
    {
        [JsonProperty(PropertyName = "item")]
        public NamedAPIResource Item { get; set; }
        [JsonProperty(PropertyName = "version_details")]
        public PokemonHeldItemVersion[] VersionDetails { get; set; }
    }
}
