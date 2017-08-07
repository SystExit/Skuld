using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class ListDescription
    {
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
    }
}
