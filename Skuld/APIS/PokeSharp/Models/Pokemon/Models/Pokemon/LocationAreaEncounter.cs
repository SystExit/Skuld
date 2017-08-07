using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class LocationAreaEncounter
    {
        [JsonProperty(PropertyName = "location_area")]
        public NamedAPIResource LocationArea { get; set; }
        [JsonProperty(PropertyName = "version_details")]
        public VersionEncounterDetail[] VersionDetails { get; set; }
    }
}
