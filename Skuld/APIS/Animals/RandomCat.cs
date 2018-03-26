using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skuld.APIS.Animals
{
    public class RandomCat
    {
        public async static Task<string> GetKittyAsync()
        {
            var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://aws.random.cat/meow"));
			if (String.IsNullOrEmpty(rawresp) || String.IsNullOrWhiteSpace(rawresp)) return null;
			dynamic item = JsonConvert.DeserializeObject(rawresp);
			var img = item["file"];
			if (img == null) return null;
			return img;
        }
    }
}
