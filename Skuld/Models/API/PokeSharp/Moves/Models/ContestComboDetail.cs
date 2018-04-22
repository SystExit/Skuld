using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Moves
{
    public class ContestComboDetail
    {
        [JsonProperty(PropertyName = "use_before")]
        public NamedAPIResource[] UseBefore { get; set; }
        [JsonProperty(PropertyName = "use_after")]
        public NamedAPIResource[] UseAfter { get; set; }
    }
}
