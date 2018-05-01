using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class ListName
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
    }
}
