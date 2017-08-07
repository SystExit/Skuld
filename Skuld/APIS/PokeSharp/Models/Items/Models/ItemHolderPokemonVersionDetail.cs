using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Items
{
    public class ItemHolderPokemonVersionDetail
    {
        [JsonProperty(PropertyName = "rarity")]
        public string Rarity { get; set; }
        [JsonProperty(PropertyName = "version")]
        public NamedAPIResource Version {get;set;}
    }
}
