using Newtonsoft.Json;

namespace PokeSharp.Models.Common
{
    public class VersionEncounterDetail
    {
        [JsonProperty(PropertyName = "version")]
        public NamedAPIResource Version { get; set; }
        [JsonProperty(PropertyName = "max_chance")]
        public int? MaxChance { get; set; }
        [JsonProperty(PropertyName = "encounter_details")]
        public Encounter[] EncounterDetails { get; set; }
    }
}
