using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class ImageNode
	{
		[JsonProperty(PropertyName = "node")]
		public Image Node { get; set; }
    }
}
