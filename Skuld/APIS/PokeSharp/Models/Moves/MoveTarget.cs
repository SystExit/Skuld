using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Moves
{
    public class MoveTarget
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public ListDescription[] Descriptions { get; set; }
        [JsonProperty(PropertyName = "moves")]
        public NamedAPIResource[] Moves { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
    }
}
