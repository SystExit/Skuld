using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class InstagramUser
	{
		[JsonProperty(PropertyName = "biography")]
		public string Biography { get; set; }

		[JsonProperty(PropertyName = "blocked_by_viewer")]
		public bool YouBlocked { get; set; }

		[JsonProperty(PropertyName = "country_block")]
		public bool CountryBlocked { get; set; }

		[JsonProperty(PropertyName = "external_url")]
		public string Website { get; set; }

		[JsonProperty(PropertyName = "followed_by")]
		public Count Followers { get; set; }

		[JsonProperty(PropertyName = "followed_by_viewer")]
		public bool YouFollow { get; set; }

		[JsonProperty(PropertyName = "follows")]
		public Count Follows { get; set; }

		[JsonProperty(PropertyName = "follows_viewer")]
		public bool FollowsYou { get; set; }

		[JsonProperty(PropertyName = "full_name")]
		public string FullName { get; set; }

		[JsonProperty(PropertyName = "has_blocked_viewer")]
		public bool BlockedYou { get; set; }

		[JsonProperty(PropertyName = "has_requested_viewer")]
		public bool RequestedFollow { get; set; }

		[JsonProperty(PropertyName = "id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "is_private")]
		public bool PrivateAccount { get; set; }

		[JsonProperty(PropertyName = "is_verified")]
		public bool VerifiedAccount { get; set; }

		[JsonProperty(PropertyName = "mutual_followers")]
		public string[] MutualFollowers { get; set; }

		[JsonProperty(PropertyName = "profile_pic_url")]
		public string ProfilePicture { get; set; }

		[JsonProperty(PropertyName = "profile_pic_url_hd")]
		public string ProfilePictureHD { get; set; }

		[JsonProperty(PropertyName = "requested_by_viewer")]
		public bool FollowRequested { get; set; }

		[JsonProperty(PropertyName = "username")]
		public string Username { get; set; }

		[JsonProperty(PropertyName = "connected_fb_page")]
		public string ConnectedFacebookPage { get; set; }

		[JsonProperty(PropertyName = "media")]
		public Media Images { get; set; }
		
		[JsonProperty(PropertyName = "logging_page_id")]
		public string LoggingPageID { get; set; }

		[JsonProperty(PropertyName = "show_suggested_profiles")]
		public bool ShowSuggestedProfiles { get; set; }
    }
}
