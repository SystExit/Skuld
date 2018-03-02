using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class Media
	{
		[JsonProperty(PropertyName = "nodes")]
		public Node[] Images { get; set; }

		[JsonProperty(PropertyName = "count")]
		public int Count { get; set; }

		[JsonProperty(PropertyName = "page_info")]
		public PageInfo PageInfo { get; set; }
	}
}
