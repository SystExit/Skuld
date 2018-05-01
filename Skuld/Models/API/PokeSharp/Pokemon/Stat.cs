using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class Stat
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "game_index")]
        public int? GameIndex { get; set; }
        [JsonProperty(PropertyName = "is_battle_only")]
        public bool IsBattleOnly { get; set; }
        [JsonProperty(PropertyName = "affecting_moves")]
        public MoveStatAffectSets AffectingMoves { get; set; }
        [JsonProperty(PropertyName = "affecting_natures")]
        public MoveStatAffectSets AffectingNatures { get; set; }
        [JsonProperty(PropertyName = "characteristics")]
        public APIResource[] Characteristics { get; set; }
        [JsonProperty(PropertyName = "move_damage_class")]
        public NamedAPIResource MoveDamageClass { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
    }
}
