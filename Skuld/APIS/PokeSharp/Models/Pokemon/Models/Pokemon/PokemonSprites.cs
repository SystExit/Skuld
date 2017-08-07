using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonSprites
    {
        [JsonProperty(PropertyName = "front_default")]
        public string Front { get; set; }
        [JsonProperty(PropertyName = "front_shiny")]
        public string FrontShiny { get; set; }
        [JsonProperty(PropertyName = "front_female")]
        public string FrontFemale { get; set; }
        [JsonProperty(PropertyName = "front_shiny_female")]
        public string FrontFemaleShiny { get; set; }
        [JsonProperty(PropertyName = "back_default")]
        public string Back { get; set; }
        [JsonProperty(PropertyName = "back_shiny")]
        public string BackShiny { get; set; }
        [JsonProperty(PropertyName = "back_female")]
        public string BackFemale { get; set; }
        [JsonProperty(PropertyName = "back_shiny_female")]
        public string BackFemaleShiny { get; set; }
    }
}
