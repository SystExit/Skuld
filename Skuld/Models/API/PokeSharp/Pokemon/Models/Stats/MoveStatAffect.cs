using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class MoveStatAffect
    {
        [JsonProperty(PropertyName = "change")]
        public int? Change { get; set; }
        [JsonProperty(PropertyName = "move")]
        public NamedAPIResource Move { get; set; }
    }
}
