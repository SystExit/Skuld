using Newtonsoft.Json;
using System;

namespace Skuld.Models.API.SysEx
{
    public class WeebGif
	{
		[JsonProperty(PropertyName = "url")]
		public string URL { get; set; }

		[JsonProperty(PropertyName = "type")]
		public Type GifType { get; set; }		
    }
	public enum Type
	{
		Adore,
		Glare,
		Grope,
		Hug,
		Kill,
		Kiss,
		Pet,
		Punch,
		Shrug,
		Slap,
		Stab
	}
}
