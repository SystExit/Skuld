using Newtonsoft.Json;

namespace Skuld.Models.API.MAL
{
    public class Manga : BaseResp
    {
		[JsonProperty(PropertyName = "chapters")]
        public string Chapters { get; internal set; }
        [JsonProperty(PropertyName = "volumes")]
        public string Volumes { get; internal set; }	
    }
}