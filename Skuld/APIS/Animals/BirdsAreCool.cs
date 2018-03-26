using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skuld.APIS.Animals
{
    public class BirdsAreCool
    {
		public static async Task<string> GetBirbAsync()
		{
			var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://birdsare.cool/bird.json"));
			dynamic data = JsonConvert.DeserializeObject(rawresp);
			var birb = data["url"];
			if(birb == null) return null;
			return birb;
		}
    }
}
