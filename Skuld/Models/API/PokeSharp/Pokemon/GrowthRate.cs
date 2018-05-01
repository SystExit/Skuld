using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class GrowthRate
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "formula")]
        public string Formula { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public ListDescription[] Descriptions { get; set; }
        [JsonProperty(PropertyName = "levels")]
        public GrowthRateExperienceLevel[] Levels { get; set; }
        [JsonProperty(PropertyName = "pokemon_species")]
        public NamedAPIResource[] PokemonSpecies { get; set; }
    }
}
