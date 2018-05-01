using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Evolution
{
    public class EvolutionDetail
    {
        [JsonProperty(PropertyName = "item")]
        public NamedAPIResource Item { get; set; }
        [JsonProperty(PropertyName = "trigger")]
        public NamedAPIResource Trigger { get; set; }
        [JsonProperty(PropertyName = "gender")]
        public int? Gender { get; set; }
        [JsonProperty(PropertyName = "helditem")]
        public NamedAPIResource HeldItem { get; set; }
        [JsonProperty(PropertyName = "known_move")]
        public NamedAPIResource KnownMove { get; set; }
        [JsonProperty(PropertyName = "known_move_type")]
        public NamedAPIResource KnownMoveType { get; set; }
        [JsonProperty(PropertyName = "location")]
        public NamedAPIResource Location { get; set; }
        [JsonProperty(PropertyName = "min_level")]
        public int? MinLevel { get; set; }
        [JsonProperty(PropertyName = "min_happiness")]
        public int? MinHappiness { get; set; }
        [JsonProperty(PropertyName = "min_beauty")]
        public int? MinBeauty { get; set; }
        [JsonProperty(PropertyName = "min_affection")]
        public int? MinAffection { get; set; }
        [JsonProperty(PropertyName = "needs_overworld_rain")]
        public bool NeedsOverworldRain { get; set; }
        [JsonProperty(PropertyName = "party_species")]
        public NamedAPIResource PartySpecies { get; set; }
        [JsonProperty(PropertyName = "party_type")]
        public NamedAPIResource PartyType { get; set; }
        [JsonProperty(PropertyName = "relative_physical_stats")]
        public int? RelativePhysicalStats { get; set; }
        [JsonProperty(PropertyName = "time_of_day")]
        public string TimeOfDay { get; set; }
        [JsonProperty(PropertyName = "trade_species")]
        public NamedAPIResource TradeSpecies { get; set; }
        [JsonProperty(PropertyName = "turn_upside_down")]
        public bool TurnUpsideDown { get; set; }
    }
}
