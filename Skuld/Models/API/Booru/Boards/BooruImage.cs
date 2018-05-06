using Newtonsoft.Json;

namespace Skuld.Models.API.Booru
{
    public class BooruImage
	{
		[JsonProperty("id")]
		public int ID { get; set; }

		[JsonProperty("score")]
		public int Score { get; set; }

		[JsonProperty("rating")]
		private string Prating { get; set; }

		[JsonProperty("file_url")]
		public string ImageUrl { get; set; }

		public Rating Rating
		{
			get
			{
				if(Prating == "s")
				{
					return Rating.Safe;
				}
				if(Prating == "q")
				{
					return Rating.Questionable;
				}
				if(Prating == "e")
				{
					return Rating.Explicit;
				}
				return Rating.None;
			}
		}
	}
}
