using PokeSharp.Models.Common;
using PokeSharp.Models.Locations;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class Location
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "region")]
        public NamedAPIResource Region { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "game_indices")]
        public GenerationGameIndex[] GameIndices { get; set; }
        [JsonProperty(PropertyName = "areas")]
        public NamedAPIResource[] Areas { get; set; }
    }
}
