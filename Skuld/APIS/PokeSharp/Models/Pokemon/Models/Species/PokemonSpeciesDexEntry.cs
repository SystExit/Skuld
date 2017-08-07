using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonSpeciesDexEntry
    {
        [JsonProperty(PropertyName = "entry_number")]
        public int? EntryNumber { get; set; }
        [JsonProperty(PropertyName = "pokedex")]
        public NamedAPIResource PokeDex { get; set; }
    }
}
