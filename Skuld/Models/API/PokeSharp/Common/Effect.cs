using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class EffectList
    {
        [JsonProperty(PropertyName = "effect")]
        public string Effect { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
    }
}
