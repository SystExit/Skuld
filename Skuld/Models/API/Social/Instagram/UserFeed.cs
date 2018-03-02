using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
	public class UserFeed
	{
		[JsonProperty(PropertyName = "user")]
		public InstagramUser Feed { get; set; }
    }
}
