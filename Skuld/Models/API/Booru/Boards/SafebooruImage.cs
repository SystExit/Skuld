using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Models.API.Booru
{
    public class SafebooruImage
	{
		[JsonProperty("directory")]
		public string Directory { get; set; }

        [JsonProperty("hash")]
		public string Hash { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }

		[JsonProperty("id")]
		public int ID { get; set; }

		[JsonProperty("image")]
		public string Image { get; set; }

		[JsonProperty("change")]		
		public ulong Change { get; set; }

		[JsonProperty("owner")]
		public string Owner { get; set; }

		[JsonProperty("parent_id")]
		public int ParentID { get; set; }

		[JsonProperty("rating")]		
		public Rating Rating { get; set; }

		[JsonProperty("sample")]		
		public bool Sample { get; set; }

		[JsonProperty("sample_height")]
		public int SampleHeight { get; set; }

		[JsonProperty("sample_width")]
		public int SampleWidth { get; set; }

		[JsonProperty("score")]
		public int Score { get; set; }

		[JsonProperty("tags")]
		private string ptags { get; set; }

		[JsonProperty("width")]
		public int Width { get; set; }

		public IReadOnlyList<string> Tags { get { return ptags.Split(' '); } }

		public virtual string ImageUrl
			=> "https://safebooru.org/images/" + Directory + "/" + Image;

		public virtual string PostUrl
			=> "https://safebooru.org/index.php?page=post&s=view&id=" + ID;
	}
}
