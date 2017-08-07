using Newtonsoft.Json;
namespace PokeSharp.Models.Items
{
    public class ItemSprites
    {
        [JsonProperty(PropertyName = "default")]
        public string Default { get; set; }
    }
}
