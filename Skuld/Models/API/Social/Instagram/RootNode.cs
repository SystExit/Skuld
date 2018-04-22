using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class RootNode
    {
		[JsonProperty(PropertyName = "entry_data")]
		public EntryData EntryData { get; set; }
	}
}
