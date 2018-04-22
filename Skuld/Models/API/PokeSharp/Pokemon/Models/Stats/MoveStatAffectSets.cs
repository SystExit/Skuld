using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class MoveStatAffectSets
    {
        [JsonProperty(PropertyName = "increase")]
        public MoveStatAffect[] Increase { get; set; }
        [JsonProperty(PropertyName = "decrease")]
        public MoveStatAffect[] Decrease { get; set; }
    }
}
