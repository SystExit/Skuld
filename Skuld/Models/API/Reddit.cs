using Newtonsoft.Json;

namespace Skuld.Models.API.Reddit
{
	public class Post
    {
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }
        [JsonProperty(PropertyName = "data")]
        public PostData Data { get; set; }
    }
    public class PostData
    {
        [JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; }
        [JsonProperty(PropertyName = "approved_at_utc")]
        public ulong? ApprovedAtUTC { get; set; }
        [JsonProperty(PropertyName = "banned_by")]
        public string BannedBy { get; set; }
        [JsonProperty(PropertyName = "media_embed")]
        public MediaEmbed MediaEmbed { get; set; }
        [JsonProperty(PropertyName = "thumbnail_width")]
        public int? ThumbnailWidth { get; set; }
        [JsonProperty(PropertyName = "subreddit")]
        public string SubReddit { get; set; }
        [JsonProperty(PropertyName = "selftext_html")]
        public string SelfTextHTML { get; set; }
        [JsonProperty(PropertyName = "selftext")]
        public string SelfText { get; set; }
        [JsonProperty(PropertyName = "likes")]
        public int? Likes { get; set; }
        [JsonProperty(PropertyName = "suggested_sort")]
        public string SuggestedSort { get; set; }
        [JsonProperty(PropertyName = "user_reports")]
        public string[] UserReports { get; set; }
        [JsonProperty(PropertyName = "secure_media")]
        public Media SecureMedia { get; set; }
        [JsonProperty(PropertyName = "is_reddit_media_domain")]
        public bool IsRedditMediaDomain { get; set; }
        [JsonProperty(PropertyName = "link_flair_text")]
        public string LinkFlairText { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }
        [JsonProperty(PropertyName = "banned_at_utc")]
        public ulong? BannedAtUTC { get; set; }
        [JsonProperty(PropertyName = "view_count")]
        public ulong? ViewCount { get; set; }
        [JsonProperty(PropertyName = "archived")]
        public bool Archived { get; set; }
        [JsonProperty(PropertyName = "clicked")]
        public bool Clicked { get; set; }
        [JsonProperty(PropertyName = "report_reasons")]
        public string ReportReasons { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "num_crossposts")]
        public int? NumCrossPosts { get; set; }
        [JsonProperty(PropertyName = "saved")]
        public bool Saved { get; set; }
        [JsonProperty(PropertyName = "mod_reports")]
        public string[] ModReports { get; set; }
        [JsonProperty(PropertyName = "can_mod_post")]
        public bool CanModPost { get; set; }
        [JsonProperty(PropertyName = "is_crosspostable")]
        public bool IsCrossPostable { get; set; }
        [JsonProperty(PropertyName = "pinned")]
        public bool IsPinned { get; set; }
        [JsonProperty(PropertyName = "score")]
        public int? Score { get; set; }
        [JsonProperty(PropertyName = "approved_by")]
        public string ApprovedBy { get; set; }
        [JsonProperty(PropertyName = "over_18")]
        public bool Over18 { get; set; }
        [JsonProperty(PropertyName = "hidden")]
        public bool Hidden { get; set; }
        [JsonProperty(PropertyName = "preview")]
        public ImagePreview Preview { get; set; }
        [JsonProperty(PropertyName = "thumbnail")]
        public string Thumbnail { get; set; }
        [JsonProperty(PropertyName = "subreddit_id")]
        public string SubRedditID { get; set; }
        [JsonProperty(PropertyName = "edited")]
        public bool Edited { get; set; }
        [JsonProperty(PropertyName = "link_flair_css_class")]
        public string LinkFlairCSSClass { get; set; }
        [JsonProperty(PropertyName = "author_flair_css_class")]
        public string AuthorFlairCSSClass { get; set; }
        [JsonProperty(PropertyName = "contest_mode")]
        public bool ContestMode { get; set; }
        [JsonProperty(PropertyName = "gilded")]
        public bool Gilded { get; set; }
        [JsonProperty(PropertyName = "downs")]
        public int? DownVotes { get; set; }
        [JsonProperty(PropertyName = "brand_safe")]
        public bool BrandSafe { get; set; }
        [JsonProperty(PropertyName = "secure_media_embed")]
        public MediaEmbed SecureMediaEmbed { get; set; }
        [JsonProperty(PropertyName = "removal_reason")]
        public string RemovalReason { get; set; }
        [JsonProperty(PropertyName = "post_hint")]
        public string PostHint { get; set; }
        [JsonProperty(PropertyName = "author_flair_text")]
        public string AuthorFlairText { get; set; }
        [JsonProperty(PropertyName = "stickied")]
        public bool Stickied { get; set; }
        [JsonProperty(PropertyName = "can_gild")]
        public bool CanGild { get; set; }
        [JsonProperty(PropertyName = "thumbnail_height")]
        public int? ThumbHeight { get; set; }
        [JsonProperty(PropertyName = "parent_whitelist_status")]
        public string ParentWhitelistStatus { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "spoiler")]
        public bool Spoiler { get; set; }
        [JsonProperty(PropertyName = "permalink")]
        public string Permalink { get; set; }
        [JsonProperty(PropertyName = "subreddit_type")]
        public string SubredditType { get; set; }
        [JsonProperty(PropertyName = "locked")]
        public bool Locked { get; set; }
        [JsonProperty(PropertyName = "hide_score")]
        public bool HideScore { get; set; }
        [JsonProperty(PropertyName = "created")]
        public ulong? Created { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "whitelist_status")]
        public string WhitelistStatus { get; set; }
        [JsonProperty(PropertyName = "quarantine")]
        public bool Quarantine { get; set; }
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; }
        [JsonProperty(PropertyName = "created_utc")]
        public ulong? CreatedUTC { get; set; }
        [JsonProperty(PropertyName = "subreddit_name_prefixed")]
        public string SubredditNamePrefixed { get; set; }
        [JsonProperty(PropertyName = "ups")]
        public int? UpVotes { get; set; }
        [JsonProperty(PropertyName = "media")]
        public Media Media { get; set; }
        [JsonProperty(PropertyName = "num_comments")]
        public int? NumberOfComments { get; set; }
        [JsonProperty(PropertyName = "is_self")]
        public bool IsSelf { get; set; }
        [JsonProperty(PropertyName = "visited")]
        public bool Visited { get; set; }
        [JsonProperty(PropertyName = "num_reports")]
        public int? NumberOfReports { get; set; }
        [JsonProperty(PropertyName = "is_video")]
        public bool IsVideo { get; set; }
        [JsonProperty(PropertyName = "distinguished")]
        public string Distinguished { get; set; }
    }
	public class SubReddit
	{
		[JsonProperty(PropertyName = "kind")]
		public string Kind { get; set; }
		[JsonProperty(PropertyName = "data")]
		public SubRedditData Data { get; set; }
	}
	public class SubRedditData
	{
		[JsonProperty(PropertyName = "modhash")]
		public string ModHash { get; set; }
		[JsonProperty(PropertyName = "whitelist_status")]
		public string WhiteListStatus { get; set; }
		[JsonProperty(PropertyName = "children")]
		public Post[] Posts { get; set; }
		[JsonProperty(PropertyName = "after")]
		public string After { get; set; }
		[JsonProperty(PropertyName = "before")]
		public string Before { get; set; }
	}
	public class ImagePreview
	{
		[JsonProperty(PropertyName = "images")]
		public ImageData[] Images { get; set; }
		[JsonProperty(PropertyName = "enabled")]
		public bool Enabled { get; set; }
	}
	public class Images
	{
		[JsonProperty(PropertyName = "source")]
		public ImageMeta Source { get; set; }
		[JsonProperty(PropertyName = "resolutions")]
		public ImageMeta[] Resolutions { get; set; }
		[JsonProperty(PropertyName = "variants")]
		public ImageVariants[] Variants { get; set; }
		[JsonProperty(PropertyName = "id")]
		public string ID { get; set; }
	}
	public class ImageData
	{
		[JsonProperty(PropertyName = "source")]
		public ImageMeta Source { get; set; }
		[JsonProperty(PropertyName = "resolutions")]
		public ImageMeta[] Resolutions { get; set; }
	}
	public class ImageMeta
	{
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
		[JsonProperty(PropertyName = "width")]
		public int Width { get; set; }
		[JsonProperty(PropertyName = "height")]
		public int Height { get; set; }
	}
	public class Media
    {
        [JsonProperty(PropertyName = "oembed")]
        public OEmbed OEmbed { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
	public class MediaEmbed
	{
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
        [JsonProperty(PropertyName = "scrolling")]
        public bool Scrolling { get; set; }
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }
        [JsonProperty(PropertyName = "media_domain_url")]
        public string MediaDomainUrl { get; set; }
    }
	public class ImageVariants
	{
		[JsonProperty(PropertyName = "obfuscated")]
		public ImageData Obfuscated { get; set; }
		[JsonProperty(PropertyName = "nsfw")]
		public ImageData NSFW { get; set; }
	}
    public class OEmbed
    {
        [JsonProperty(PropertyName = "provider_url")]
        public string ProviderUrl { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "thumbnail_width")]
        public int ThumbWidth { get; set; }
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }
        [JsonProperty(PropertyName = "html")]
        public string HTML { get; set; }
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
        [JsonProperty(PropertyName = "provider_name")]
        public string ProviderName { get; set; }
        [JsonProperty(PropertyName = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }
        [JsonProperty(PropertyName = "thumbnail_height")]
        public int ThumbHeight { get; set; }
    }
}
