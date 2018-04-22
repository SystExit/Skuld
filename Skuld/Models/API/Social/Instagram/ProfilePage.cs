using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class ProfilePage
	{
		[JsonProperty(PropertyName = "graphql")]
		public UserFeed Feeds { get; set; }
	}
}
