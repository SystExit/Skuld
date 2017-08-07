using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class FlavorTextList
    {
        [JsonProperty(PropertyName = "flavor_text")]
        public string FlavorText { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
    }
}
