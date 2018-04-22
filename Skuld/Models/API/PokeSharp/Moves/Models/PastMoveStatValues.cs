using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Moves
{
    public class PastMoveStatValues
    {
        [JsonProperty(PropertyName = "accuracy")]
        public int? Accuracy { get; set; }
        [JsonProperty(PropertyName = "effect_chance")]
        public int? EffectChance { get; set; }
        [JsonProperty(PropertyName = "power")]
        public int? Power { get; set; }
        [JsonProperty(PropertyName = "pp")]
        public int? PP { get; set; }
        [JsonProperty(PropertyName = "effect_entries")]
        public VerboseEffect[] EffectEntries { get; set; }
        [JsonProperty(PropertyName = "type")]
        public NamedAPIResource Type { get; set; }
        [JsonProperty(PropertyName = "version_group")]
        public NamedAPIResource VersionGroup { get; set; }
    }
}
