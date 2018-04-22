using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class NaturePokeathlonStatAffectSets
    {
        [JsonProperty(PropertyName = "increase")]
        public NaturePokeathlonStatAffect[] Increase { get; set; }
        [JsonProperty(PropertyName = "decrease")]
        public NaturePokeathlonStatAffect[] Decrease { get; set; }
    }
}
