using PokeSharp.Models.Utility;
using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class VerboseEffect
    {
        [JsonProperty(PropertyName = "effect")]
        public string Effect { get;  set; }
        [JsonProperty(PropertyName = "short_effect")]
        public string ShortEffect { get;  set; }
        [JsonProperty(PropertyName = "language")]
        public Languages Language { get; set; }
    }
}
