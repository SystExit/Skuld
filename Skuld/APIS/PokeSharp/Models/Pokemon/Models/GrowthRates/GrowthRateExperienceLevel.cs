using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class GrowthRateExperienceLevel
    {
        [JsonProperty(PropertyName = "level")]
        public int? Level { get; set; }
        [JsonProperty(PropertyName = "experience")]
        public int? Experience { get; set; }
    }
}
