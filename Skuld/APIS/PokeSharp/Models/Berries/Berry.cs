using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Berries
{
    public class Berry
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get;  set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get;  set; }
        [JsonProperty(PropertyName = "growth_time")]
        public int? GrowthTime { get;  set; }
        [JsonProperty(PropertyName = "max_harvest")]
        public int? MaxHarvest { get;  set; }
        [JsonProperty(PropertyName = "natural_gift_power")]
        public int? NaturalGiftPower { get;  set; }
        [JsonProperty(PropertyName = "size")]
        public int? Size { get;  set; }
        [JsonProperty(PropertyName = "smoothness")]
        public int? Smoothness { get;  set; }
        [JsonProperty(PropertyName = "soil_dryness")]
        public int? SoilDryness { get; set; }
        [JsonProperty(PropertyName = "firmness")]
        public NamedAPIResource Firmness { get; set; }
        [JsonProperty(PropertyName = "flavors")]
        public BerryFlavorMap[] Flavors { get; set; }
        [JsonProperty(PropertyName = "item")]
        public NamedAPIResource Item { get; set; }
        [JsonProperty(PropertyName = "natural_gift_type")]
        public NamedAPIResource NaturalGiftType { get; set; }
    }
}
