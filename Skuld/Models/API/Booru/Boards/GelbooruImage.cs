using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skuld.Models.API.Booru
{
    public class GelbooruImage : BooruImage
	{
		[JsonProperty("directory")]
		public string Directory { get; set; }
				
		[JsonProperty("tags")]
		private string Ptags { get; set; }
		
		public IReadOnlyList<string> Tags { get { return Ptags.Split(' '); } }
		
		public virtual string PostUrl { get { return "https://gelbooru.com/index.php?page=post&s=view&id=" + ID; } }
	}
}