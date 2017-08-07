using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Locations
{
    public class EncounterVersionDetails
    {
        [JsonProperty(PropertyName = "rate")]
        public int? Rate { get; set; }
        [JsonProperty(PropertyName = "version")]
        public NamedAPIResource Version { get; set; }
    }
}
