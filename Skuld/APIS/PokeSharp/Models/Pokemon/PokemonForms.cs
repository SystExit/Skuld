using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonForms
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "order")]
        public int? Order { get; set; }
        [JsonProperty(PropertyName = "form_order")]
        public int? FormOrder { get; set; }
        [JsonProperty(PropertyName = "is_default")]
        public bool IsDefault { get; set; }
        [JsonProperty(PropertyName = "is_battle_only")]
        public bool IsBattleOnly { get; set; }
        [JsonProperty(PropertyName = "is_mega")]
        public bool IsMega { get; set; }
        [JsonProperty(PropertyName = "form_name")]
        public string FormName { get; set; }
        [JsonProperty(PropertyName = "pokemon")]
        public NamedAPIResource Pokemon { get; set; }
        [JsonProperty(PropertyName = "sprites")]
        public PokemonFormSprites Sprites { get; set; }
        [JsonProperty(PropertyName = "version_group")]
        public NamedAPIResource VersionGroup { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "form_names")]
        public ListName[] FormNames { get; set; }
    }
}
