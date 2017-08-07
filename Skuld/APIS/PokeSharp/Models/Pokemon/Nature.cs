using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class Nature
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "decreased_stat")]
        public NamedAPIResource DecreasedStat { get; set; }
        [JsonProperty(PropertyName = "increased_stat")]
        public NamedAPIResource IncreasedStat { get; set; }
        [JsonProperty(PropertyName = "hates_flavor")]
        public NamedAPIResource HatesFlavor { get; set; }
        [JsonProperty(PropertyName = "likes_flavor")]
        public NamedAPIResource LikesFlavor { get; set; }
        [JsonProperty(PropertyName = "pokeathlon_stat_changes")]
        public NatureStatChange[] PokeathlonStatChanges { get; set; }
        [JsonProperty(PropertyName = "move_battle_style_preferences")]
        public MoveBattleStylePreference[] MoveBattleStylePreferences { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
    }
}
