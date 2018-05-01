using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Locations
{
    public class PalParkAreas
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "pokemon_encounters")]
        public PalParkEncounterSpecies[] PokemonEncounters { get; set; }
    }
}
