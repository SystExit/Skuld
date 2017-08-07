using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class SuperContestEffects
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "appeal")]
        public int? Appeal { get; set; }
        [JsonProperty(PropertyName = "flavor_text_entries")]
        public FlavorTextList[] FlavorTextEntries { get; set; }
        [JsonProperty(PropertyName = "moves")]
        public NamedAPIResource[] Moves { get; set; }
    }
}
