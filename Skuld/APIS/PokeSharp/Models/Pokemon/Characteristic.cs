using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class Characteristic
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "gene_modulo")]
        public int? GeneModulo { get; set; }
        [JsonProperty(PropertyName = "possible_values")]
        public int?[] PossibleValues { get; set; }
        [JsonProperty(PropertyName = "descriptions")]
        public ListDescription[] Descriptions { get; set; }
    }
}
