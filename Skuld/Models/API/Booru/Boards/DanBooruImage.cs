using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Models.API.Booru
{
    public class DanbooruImage
	{
		[JsonProperty("id")]
		public int ID { get; set; }

		[JsonProperty("score")]
		public int Score { get; set; }

		[JsonProperty("rating")]
		private string Prating { get; set; }
		
		[JsonProperty("tag_string")]
		private string TagString { get; set; }

		[JsonProperty("file_url")]
		public string ImageUrl { get; set; }

		public IReadOnlyList<string> Tags { get { return TagString.Split(' '); } }

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

		public virtual string PostUrl { get { return "https://danbooru.donmai.us/posts/" + ID; } }
    }
}
