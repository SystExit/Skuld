using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Moves
{
    public class MoveMetaData
    {
        [JsonProperty(PropertyName = "ailment")]
        public NamedAPIResource Ailment { get; set; }
        [JsonProperty(PropertyName = "category")]
        public NamedAPIResource Category { get; set; }
        [JsonProperty(PropertyName = "min_hits")]
        public int? MinHits { get; set; }
        [JsonProperty(PropertyName = "max_hits")]
        public int? MaxHits { get; set; }
        [JsonProperty(PropertyName = "min_turns")]
        public int? MinTurns { get; set; }
        [JsonProperty(PropertyName = "max_turns")]
        public int? MaxTurns { get; set; }
        [JsonProperty(PropertyName = "drain")]
        public int? Drain { get; set; }
        [JsonProperty(PropertyName = "healing")]
        public int? Healing { get; set; }
        [JsonProperty(PropertyName = "crit_rate")]
        public int? CritRate { get; set; }
        [JsonProperty(PropertyName = "ailment_chance")]
        public int? AilmentChance { get; set; }
        [JsonProperty(PropertyName = "flinch_chance")]
        public int? FlinchChance { get; set; }
        [JsonProperty(PropertyName = "stat_chance")]
        public int? StatChance { get; set; }
    }
}
