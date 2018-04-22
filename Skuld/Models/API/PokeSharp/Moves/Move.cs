using PokeSharp.Models.Common;
using PokeSharp.Models.Moves;
using PokeSharp.Models.Pokemon;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class Move
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "accuracy")]
        public int? Accuracy { get; set; }
        [JsonProperty(PropertyName = "effect_chance")]
        public int? EffectChance { get; set; }
        [JsonProperty(PropertyName = "pp")]
        public int? PP { get; set; }
        [JsonProperty(PropertyName = "priority")]
        public int? Priority { get; set; }
        [JsonProperty(PropertyName = "power")]
        public int? Power { get; set; }
        [JsonProperty(PropertyName = "contest_combos")]
        public ContestComboSets ContestCombos { get; set; }
        [JsonProperty(PropertyName = "combo_type")]
        public NamedAPIResource ContestType { get; set; }
        [JsonProperty(PropertyName = "contest_effect")]
        public APIResource ContestEffect { get; set; }
        [JsonProperty(PropertyName = "damage_class")]
        public NamedAPIResource DamageClass { get; set; }
        [JsonProperty(PropertyName = "effect_entries")]
        public VerboseEffect[] EffectEntries { get; set; }
        [JsonProperty(PropertyName = "effect_changes")]
        public AbilityEffectChange[] EffectChanges { get; set; }
        [JsonProperty(PropertyName = "flavor_text_entries")]
        public Move FlavorTextEntries { get; set; }
        [JsonProperty(PropertyName = "generation")]
        public NamedAPIResource Generation { get; set; }
        [JsonProperty(PropertyName = "machines")]
        public MachineVersionDetail[] Machines { get; set; }
        [JsonProperty(PropertyName = "meta")]
        public MoveMetaData Meta { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "past_values")]
        public PastMoveStatValues PastValues {get; set; }
        [JsonProperty(PropertyName = "stat_changes")]
        public MoveStatChange[] StatChanges { get; set; }
        [JsonProperty(PropertyName = "target")]
        public NamedAPIResource Target { get; set; }
        [JsonProperty(PropertyName = "type")]
        public NamedAPIResource Type { get; set; }
    }
}
