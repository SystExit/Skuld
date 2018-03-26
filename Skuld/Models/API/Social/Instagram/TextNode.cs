using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class TextNode
    {
		public string Text { get => caption.Text; }

		[JsonProperty(PropertyName = "node")]
		InstaText caption { get; set; }
	}
}
