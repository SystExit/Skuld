using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class Node
	{
		[JsonProperty(PropertyName = "__typename")]
		public string TypeName { get; set; }

		[JsonProperty(PropertyName = "id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "comments_disabled")]
		public bool CommentsDisabled { get; set; }

		[JsonProperty(PropertyName = "dimensions")]
		public Dimensions Dimensions { get; set; }

		[JsonProperty(PropertyName = "edge_media_preview_like")]
		public Count MediaPreviewLike { get; set; }

		[JsonProperty(PropertyName = "media_preview")]
		public string MediaPreview { get; set; }

		[JsonProperty(PropertyName = "owner")]
		public Owner Owner { get; set; }

		[JsonProperty(PropertyName = "thumbnail_src")]
		public string ThumbnailSrc { get; set; }

		[JsonProperty(PropertyName = "thumbnail_resources")]
		public Thumbnails[] ThumbnailResources { get; set; }

		[JsonProperty(PropertyName = "is_video")]
		public bool IsVideo { get; set; }

		[JsonProperty(PropertyName = "code")]
		public string Code { get; set; }

		[JsonProperty(PropertyName = "date")]
		public ulong Date { get; set; }

		[JsonProperty(PropertyName = "display_src")]
		public string DisplaySrc { get; set; }

		[JsonProperty(PropertyName = "caption")]
		public string Caption { get; set; }

		[JsonProperty(PropertyName = "comments")]
		public Count CommentCount { get; set; }

		[JsonProperty(PropertyName = "likes")]
		public Count Likes { get; set; }
    }
}
