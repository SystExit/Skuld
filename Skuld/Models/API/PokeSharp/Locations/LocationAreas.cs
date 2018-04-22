using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Locations
{
    public class LocationAreas
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "game_index")]
        public int? GameIndex { get; set; }
        [JsonProperty(PropertyName = "encounter_method_rates")]
        public EncounterMethodRate[] EncounterMethodRates { get; set; }
        [JsonProperty(PropertyName = "location")]
        public NamedAPIResource Location { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "pokemon_encounters")]
        public PokemonEncounter[] PokemonEncounters { get; set; }
    }
}
