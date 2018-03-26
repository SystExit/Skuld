using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Skuld.APIS.Animals
{
    public class NekoLife
	{
		static async Task<string> GetImageAsync(Uri uri)
		{
			var rawresp = await WebHandler.ReturnStringAsync(uri);
			dynamic item = JObject.Parse(rawresp);
			var img = item["url"];
			if (img == null) return null;
			return img;
		}

		public static async Task<string> GetNekoAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/neko"));

		public static async Task<string> GetLewdNekoAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd"));

		public static async Task<string> GetKissAsync() =>		
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kiss"));
		
		public static async Task<string> GetHugAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hug"));

		public static async Task<string> GetPatAsync() =>		
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pat"));

		public static async Task<string> GetCuddleAsync() =>		
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cuddle"));		

		public static async Task<string> GetLizardAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lizard"));
	}
}
