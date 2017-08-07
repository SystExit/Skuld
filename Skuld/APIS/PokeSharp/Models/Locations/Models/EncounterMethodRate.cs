using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Locations
{
    public class EncounterMethodRate
    {
        [JsonProperty(PropertyName = "encounter_method")]
        public NamedAPIResource EncounterMethod { get; set; }
        [JsonProperty(PropertyName = "version_details")]
        public EncounterVersionDetails[] VersionDetails { get; set; }
    }
}
