using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class EntryData
    {
        [JsonProperty(PropertyName = "ProfilePage")]
        public List<ProfilePage> ProfilePages { get; set; }
    }
}