using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonMoveVersion
    {
        [JsonProperty(PropertyName = "move_learn_method")]
        public NamedAPIResource MoveLearnMethod { get; set; }
        [JsonProperty(PropertyName = "version_group")]
        public NamedAPIResource VersionGroup { get; set; }
        [JsonProperty(PropertyName = "level_learned_at")]
        public int? LevelLearnedAt { get; set; }
    }
}
