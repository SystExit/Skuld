using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Moves
{
    public class MoveStatChange
    {
        [JsonProperty(PropertyName = "change")]
        public int? Change { get; set; }
        [JsonProperty(PropertyName = "stat")]
        public NamedAPIResource Stat { get; set; }
    }
}
