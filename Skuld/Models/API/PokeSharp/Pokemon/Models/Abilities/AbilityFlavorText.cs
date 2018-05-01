using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class AbilityFlavorText
    {
        [JsonProperty(PropertyName = "flavor_text")]
        public string FlavorText { get; set; }
        [JsonProperty(PropertyName = "language")]
        public NamedAPIResource Language { get; set; }
        [JsonProperty(PropertyName = "version_group")]
        public NamedAPIResource VersionGroup { get; set; }
    }
}
