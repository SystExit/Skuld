using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class PageInfo
    {
        [JsonProperty(PropertyName = "has_next_page")]
        public bool HasNextPage { get; set; }

        [JsonProperty(PropertyName = "end_cursor")]
        public string EndCursor { get; set; }
    }
}