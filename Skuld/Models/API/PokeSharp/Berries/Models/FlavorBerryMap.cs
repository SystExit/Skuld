using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Berries
{
    public class FlavorBerryMap
    {
        [JsonProperty(PropertyName = "potency")]
        public int? Potency { get; set; }
        [JsonProperty(PropertyName = "berry")]
        public NamedAPIResource Berry { get; set; }
    }
}
