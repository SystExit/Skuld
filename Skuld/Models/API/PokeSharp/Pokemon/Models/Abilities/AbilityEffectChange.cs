using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class AbilityEffectChange
    {
        [JsonProperty(PropertyName = "effect_entries")]
        public EffectList[] EffectEntries { get; set; }
        [JsonProperty(PropertyName = "version_group")]
        public NamedAPIResource VersionGroup { get; set; }
    }
}
