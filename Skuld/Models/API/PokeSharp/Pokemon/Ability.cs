using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class Ability
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "is_main_series")]
        public bool IsMainSeries { get; set; }
        [JsonProperty(PropertyName = "generation")]
        public NamedAPIResource Generation { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "effect_entries")]
        public VerboseEffect[] EffectEntries { get; set; }
        [JsonProperty(PropertyName = "effect_changes")]
        public AbilityEffectChange[] EffectChanges { get; set; }
        [JsonProperty(PropertyName = "flavor_text_entries")]
        public AbilityFlavorText[] FlavorTextEntries { get; set; }
        [JsonProperty(PropertyName = "pokemon")]
        public AbilityPokemon[] Pokemon { get; set; }
    }
}
