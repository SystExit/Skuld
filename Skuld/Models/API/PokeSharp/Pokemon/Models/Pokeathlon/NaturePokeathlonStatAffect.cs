using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class NaturePokeathlonStatAffect
    {
        [JsonProperty(PropertyName = "max_change")]
        public int? MaxChange { get; set; }
        [JsonProperty(PropertyName = "nature")]
        public NamedAPIResource Nature { get; set; }
    }
}
