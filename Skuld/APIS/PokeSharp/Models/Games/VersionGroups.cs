using PokeSharp.Models.Common;
using PokeSharp.Models.Games;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class VersionGroups
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "order")]
        public int? Order { get; set; }
        [JsonProperty(PropertyName = "generation")]
        public NamedAPIResource Generation { get; set; }
        [JsonProperty(PropertyName = "move_learn_methods")]
        public NamedAPIResource[] MoveLearnMethods { get; set; }
        [JsonProperty(PropertyName = "pokedexes")]
        public NamedAPIResource[] Pokedexes { get; set; }
        [JsonProperty(PropertyName = "regions")]
        public NamedAPIResource[] Regions { get; set; }
        [JsonProperty(PropertyName = "versions")]
        public NamedAPIResource[] Versions { get; set; }
    }
}
