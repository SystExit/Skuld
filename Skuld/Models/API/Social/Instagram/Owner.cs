using Newtonsoft.Json;

namespace Skuld.Models.API.Social.Instagram
{
    public class Owner
	{
		[JsonProperty(PropertyName = "id")]
		public string ID { get; set; }
    }
}
