using Newtonsoft.Json;

namespace Skuld.Models.API.SysEx
{
    public class Animal
	{
		[JsonProperty(PropertyName = "file")]
		public string URL { get; set; }
	}
}
