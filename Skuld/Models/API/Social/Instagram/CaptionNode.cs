using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Models.API.Social.Instagram
{
    public class CaptionNode
    {
		[JsonProperty(PropertyName = "edges")]
		public List<TextNode> Captions { get; set; }
    }
}
