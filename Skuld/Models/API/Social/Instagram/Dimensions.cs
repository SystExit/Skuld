using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class Dimensions
	{
		[JsonProperty(PropertyName = "height")]
		public int Height { get; set; }

		[JsonProperty(PropertyName = "width")]
		public int Width { get; set; }
	}
}
