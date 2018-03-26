using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Models.API.Social.Instagram
{
    public class Media
	{
		[JsonProperty(PropertyName = "edges")]
		public List<ImageNode> Images { get; set; }

		[JsonProperty(PropertyName = "count")]
		public int Count { get; set; }

		[JsonProperty(PropertyName = "page_info")]
		public PageInfo PageInfo { get; set; }
	}
}
