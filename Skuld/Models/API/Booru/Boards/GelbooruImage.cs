using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Models.API.Booru
{
    public class GelbooruImage
	{
		[JsonProperty("directory")]
		public string Directory { get; set; }

		[JsonProperty("id")]
		public int ID { get; set; }

		[JsonProperty("image")]
		public string Image { get; set; }

		[JsonProperty("rating")]
		private string Prating { get; set; }

		[JsonProperty("score")]
		public int Score { get; set; }

		[JsonProperty("tags")]
		private string Ptags { get; set; }

		[JsonProperty("file_url")]
		public string ImageUrl { get; set; }

		public IReadOnlyList<string> Tags { get { return Ptags.Split(' '); } }

		public Rating Rating
		{
			get
			{
				switch (Prating)
				{
					case "s":
						return Rating.Safe;
					case "q":
						return Rating.Questionable;
					case "e":
						return Rating.Explicit;
				}
				return Rating.None;
			}
		}

		public virtual string PostUrl { get { return "https://gelbooru.com/index.php?page=post&s=view&id=" + ID; } }
	}
}