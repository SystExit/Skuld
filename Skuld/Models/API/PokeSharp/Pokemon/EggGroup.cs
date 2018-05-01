using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class EggGroup
    {
        [JsonProperty(PropertyName = "id")]
		public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "pokemon_species")]
        public NamedAPIResource[] PokemonSpecies { get; set; }
    }
}
