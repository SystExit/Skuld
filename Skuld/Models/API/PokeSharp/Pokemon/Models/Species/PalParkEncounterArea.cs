using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PalParkEncounterArea
    {
        [JsonProperty(PropertyName = "base_score")]
        public int? BaseScore { get; set; }
        [JsonProperty(PropertyName = "rate")]
        public int? Rate { get; set; }
        [JsonProperty(PropertyName = "area")]
        public NamedAPIResource Area { get; set; }
    }
}
