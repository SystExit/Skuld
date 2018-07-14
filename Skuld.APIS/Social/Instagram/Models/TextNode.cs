using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class TextNode
    {
        public string Text { get => caption.Text; }

        [JsonProperty(PropertyName = "node")]
        private InstaText caption { get; set; }
    }
}