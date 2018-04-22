using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Items
{
    public class ItemFlingEffects
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "effect_entries")]
        public EffectList[] EffectEntries { get; set; }
        [JsonProperty(PropertyName = "items")]
        public NamedAPIResource[] Items { get; set; }
    }
}
