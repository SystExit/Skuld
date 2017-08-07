using Newtonsoft.Json;

namespace PokeSharp.Models.Common
{
    public class Encounter
    {
        [JsonProperty(PropertyName = "min_level")]
        public int? MinLevel { get; set; }
        [JsonProperty(PropertyName = "max_level")]
        public int? MaxLevel { get; set; }
        [JsonProperty(PropertyName = "condition_values")]
        public NamedAPIResource[] ConditionValues { get; set; }
        [JsonProperty(PropertyName = "chance")]
        public int? Chance { get; set; }
        [JsonProperty(PropertyName = "method")]
        public NamedAPIResource Method { get; set; }
    }
}
