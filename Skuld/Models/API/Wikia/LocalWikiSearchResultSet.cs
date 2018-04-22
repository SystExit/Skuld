using Newtonsoft.Json;

namespace Skuld.Models.API.Wikia
{
    public class LocalWikiSearchResultSet
	{
		[JsonProperty(PropertyName = "total")]
		public int Total { get; set; }
		[JsonProperty(PropertyName = "batches")]
		public int Batches { get; set; }
		[JsonProperty(PropertyName = "currentBatch")]
		public string CurrentBatch { get; set; }
		[JsonProperty(PropertyName = "next")]
		public int Next { get; set; }
		[JsonProperty(PropertyName = "items")]
		public LocalWikiSearchResult[] Items { get; set; }
    }
}
