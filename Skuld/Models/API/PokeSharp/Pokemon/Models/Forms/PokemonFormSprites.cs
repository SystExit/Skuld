using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonFormSprites
    {
        [JsonProperty(PropertyName = "front_default")]
        public string Front { get; set; }
        [JsonProperty(PropertyName = "front_shiny")]
        public string FrontShiny { get; set; }
        [JsonProperty(PropertyName = "back_default")]
        public string Back { get; set; }
        [JsonProperty(PropertyName = "back_shiny")]
        public string BackShiny { get; set; }
    }
}
