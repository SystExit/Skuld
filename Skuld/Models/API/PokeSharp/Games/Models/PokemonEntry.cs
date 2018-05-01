using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Games
{
    public class PokemonEntry
    {
        [JsonProperty(PropertyName = "entry_number")]
        public int? EntryNumber { get; set; }
        [JsonProperty(PropertyName = "pokemon_species")]
        public NamedAPIResource PokemonSpecies { get; set; }
    }
}
