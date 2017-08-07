using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class NamedAPIResource
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
