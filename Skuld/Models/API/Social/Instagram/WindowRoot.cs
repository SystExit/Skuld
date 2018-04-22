using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class WindowRoot
    {
		[JsonProperty(PropertyName = "entry_data")]
		public InstagramUser User { get; set; }
	}
}
