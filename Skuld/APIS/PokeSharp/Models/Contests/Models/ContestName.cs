using PokeSharp.Models.Common;
using Newtonsoft.Json;
namespace PokeSharp.Models.Contests
{
    public class ContestName
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
    }
}
