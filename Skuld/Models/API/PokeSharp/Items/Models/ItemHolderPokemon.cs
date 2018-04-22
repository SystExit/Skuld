using Newtonsoft.Json;
namespace PokeSharp.Models.Items
{
    public class ItemHolderPokemon
    {
        [JsonProperty(PropertyName = "pokemon")]
        public string Pokemon { get; set; }
        [JsonProperty(PropertyName = "version_details")]
        public ItemHolderPokemonVersionDetail[] VersionDetails { get; set; }
    }
}
