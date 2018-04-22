using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class ListGenus
    {
        [JsonProperty(PropertyName = "genus")]
        public string Genus { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
    }
}
