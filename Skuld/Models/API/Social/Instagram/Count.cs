using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class Count
    {
		[JsonProperty(PropertyName = "count")]
		public int Value { get; set; }
    }
}
