using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class RootNode
    {
		[JsonProperty(PropertyName = "graphql")]
		public UserFeed Feed { get; set; }

		[JsonProperty(PropertyName = "logging_page_id")]
		public string LoggingPageID { get; set; }

		[JsonProperty(PropertyName = "show_suggested_profiles")]
		public bool ShowSuggestedProfiles { get; set; }
	}
}
