using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Models.API.Social.Instagram
{
    public class EntryData
    {
		[JsonProperty(PropertyName = "ProfilePage")]
		public List<ProfilePage> ProfilePages { get; set; }
	}
}
