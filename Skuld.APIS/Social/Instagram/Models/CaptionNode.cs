using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class CaptionNode
    {
        [JsonProperty(PropertyName = "edges")]
        public List<TextNode> Captions { get; set; }
    }
}