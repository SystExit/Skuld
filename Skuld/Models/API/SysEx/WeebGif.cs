using Newtonsoft.Json;
using System;

namespace Skuld.Models.API.SysEx
{
    public class WeebGif
	{
		[JsonProperty(PropertyName = "url")]
		public string URL { get; set; }

		[JsonProperty(PropertyName = "type")]
		public GifType GifType { get; set; }		
    }
	public enum GifType
	{
		Adore,
		Amazed,
		Approve,
		Blush,
		Bored,
		Cuddle,
		Dance,
		Disgust,
		Disapprove,
		Feels,
		Glare,
		Grope,
		Happy,
		Hug,
		Kill,
		Kiss,
		Lewd,
		Mad,
		Moe,
		Nani,
		Nope,
		Pet,
		Punch,
		Sad,
		Shock,
		Shrug,
		Slap,
		Smug,
		Stab,
		Triggered,
		Unamused
	}
}
