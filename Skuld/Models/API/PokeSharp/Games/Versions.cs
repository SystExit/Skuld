using PokeSharp.Models.Common;
using PokeSharp.Models.Games;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class Versions
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "version_group")]
        public NamedAPIResource VersionGroup { get; set; }
    }
}
