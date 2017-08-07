using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class ListAwesomeName
    {
        [JsonProperty(PropertyName = "awesome_name")]
        public string AwesomeName { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
    }
}
