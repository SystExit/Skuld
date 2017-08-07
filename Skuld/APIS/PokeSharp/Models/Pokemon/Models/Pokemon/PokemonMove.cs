using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonMove
    {
        [JsonProperty(PropertyName = "move")]
        public NamedAPIResource Move { get; set; }
        [JsonProperty(PropertyName = "version_group_details")]
        public PokemonMoveVersion[] VersionGroupDetails { get; set; }
    }
}
