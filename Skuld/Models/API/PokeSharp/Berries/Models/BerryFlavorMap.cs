using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Berries
{
    public class BerryFlavorMap
    {
        [JsonProperty(PropertyName = "potency")]
        public int? Potency { get; set; }
        [JsonProperty(PropertyName = "flavor")]
        public NamedAPIResource Flavor { get; set; }
    }
}
