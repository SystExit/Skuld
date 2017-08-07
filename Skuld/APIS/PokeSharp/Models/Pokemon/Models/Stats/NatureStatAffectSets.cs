using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class NatureStatAffectSets
    {
        [JsonProperty(PropertyName = "increase")]
        public NamedAPIResource[] Increase { get; set; }
        [JsonProperty(PropertyName = "decrease")]
        public NamedAPIResource[] Decrease { get; set; }
    }
}
