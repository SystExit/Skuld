using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class MoveBattleStylePreference
    {
        [JsonProperty(PropertyName = "low_hp_preference")]
        public int? LowHPPreference { get; set; }

        [JsonProperty(PropertyName = "high_hp_preference")]
        public int? HighHPPreference { get; set; }
        [JsonProperty(PropertyName = "move_battle_style")]
        public NamedAPIResource MoveBattleStyle { get; set; }
    }
}
