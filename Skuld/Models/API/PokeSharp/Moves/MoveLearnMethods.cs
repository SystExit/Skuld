using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Moves
{
    public class MoveLearnMethods
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public ListDescription[] Descriptions { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "version_groups")]
        public NamedAPIResource[] VersionGroups { get; set; }
    }
}
