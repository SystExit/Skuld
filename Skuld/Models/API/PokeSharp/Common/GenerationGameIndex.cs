using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class GenerationGameIndex
    {
        [JsonProperty(PropertyName = "game_index")]
        public int? GameIndex { get; set; }
        [JsonProperty(PropertyName = "generation")]
        public NamedAPIResource Generation { get; set; }
    }
}
