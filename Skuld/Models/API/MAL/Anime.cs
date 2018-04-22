using Newtonsoft.Json;

namespace Skuld.Models.API.MAL
{
    public class Anime : BaseResp
	{
        [JsonProperty(PropertyName = "episodes")]
        public string Episodes { get; internal set; }
    }
}
