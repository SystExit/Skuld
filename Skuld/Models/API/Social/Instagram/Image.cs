using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace Skuld.Models.API.Social.Instagram
{
    public class Image
	{
		[JsonProperty(PropertyName = "__typename")]
		public string TypeName { get; set; }

		[JsonProperty(PropertyName = "id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "edge_media_to_caption")]
		CaptionNode captions { get; set; }

		public List<TextNode> Captions { get => captions.Captions; }
		
		public string PrimaryCaption { get => Captions.FirstOrDefault().Text;}

		[JsonProperty(PropertyName = "shortcode")]
		public string Code { get; set; }

		[JsonProperty(PropertyName = "edge_media_to_comment")]
		public Count CommentCount { get; set; }

		[JsonProperty(PropertyName = "comments_disabled")]
		public bool CommentsDisabled { get; set; }

		[JsonProperty(PropertyName = "taken_at_timestamp")]
		public ulong Date { get; set; }

		[JsonProperty(PropertyName = "dimensions")]
		public Dimensions Dimensions { get; set; }

		[JsonProperty(PropertyName = "display_url")]
		public string DisplaySrc { get; set; }

		//gating_info

		[JsonProperty(PropertyName = "media_preview")]
		public string MediaPreview { get; set; }

		[JsonProperty(PropertyName = "owner")]
		public Owner Owner { get; set; }

		[JsonProperty(PropertyName = "thumbnail_src")]
		public string ThumbnailSrc { get; set; }

		[JsonProperty(PropertyName = "thumbnail_resources")]
		public List<Thumbnails> ThumbnailResources { get; set; }

		[JsonProperty(PropertyName = "is_video")]
		public bool IsVideo { get; set; }
		
		[JsonProperty(PropertyName = "edge_liked_by")]
		public Count Likes { get; set; }

		[JsonProperty(PropertyName = "edge_media_preview_like")]
		public Count MediaPreviewLike { get; set; }
	}
}
