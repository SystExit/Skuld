using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Locations
{
    public class PokemonEncounter
    {
        [JsonProperty(PropertyName = "pokemon")]
        public NamedAPIResource Pokemon { get; set; }
        [JsonProperty(PropertyName = "version_details")]
        public VersionEncounterDetail[] VersionDetails { get; set; }
    }
}
