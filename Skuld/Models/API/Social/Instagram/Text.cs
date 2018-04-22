using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class InstaText
    {
		[JsonProperty(PropertyName = "text")]
		public string Text { get; set; }
	}
}
