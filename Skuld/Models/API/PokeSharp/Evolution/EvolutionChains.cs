using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Evolution
{
    public class EvolutionChains
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "baby_trigger_item")]
        public NamedAPIResource BabyTriggerItem { get; set; }
        [JsonProperty(PropertyName = "chain")]
        public ChainLink Chain { get; set; }
    }
}
