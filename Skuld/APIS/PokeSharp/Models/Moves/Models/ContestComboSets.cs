using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Moves
{
    public class ContestComboSets
    {
        [JsonProperty(PropertyName = "normal")]
        public ContestComboDetail Normal { get; set; }
        [JsonProperty(PropertyName = "super")]
        public ContestComboDetail Super { get; set; }
    }
}
