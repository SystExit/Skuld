using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class ContestEffects
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "appeal")]
        public int? Appeal { get; set; }
        [JsonProperty(PropertyName = "jam")]
        public int? Jam { get; set; }
        [JsonProperty(PropertyName = "effect_entries")]
        public EffectList[] EffectEntries { get; set; }
        [JsonProperty(PropertyName = "flavor_text_entries")]
        public FlavorTextList[] FlavorTextEntries { get; set; }
    }
}
