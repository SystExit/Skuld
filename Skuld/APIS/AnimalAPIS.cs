using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Skuld.Services;

namespace Skuld.APIS
{
    public class AnimalAPIS
    {
		readonly LoggingService logger;
		public AnimalAPIS(LoggingService log) //depinj
		{
			logger = log;
		}

		//birds are cool
		public async Task<string> GetBirbAsync()
		{
			var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://birdsare.cool/bird.json"));
			dynamic data = JsonConvert.DeserializeObject(rawresp);
			var birb = data["url"];
			if (birb == null) return null;
			return birb;
		}

		//NekoLife
		async Task<string> GetImageAsync(Uri uri)
		{
			var rawresp = await WebHandler.ReturnStringAsync(uri);
			dynamic item = JObject.Parse(rawresp);
			var img = item["url"];
			if (img == null) return null;
			return img;
		}

		public async Task<string> GetNekoAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/neko"));

		public async Task<string> GetLewdNekoAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd"));

		public async Task<string> GetKissAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kiss"));

		public async Task<string> GetHugAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hug"));

		public async Task<string> GetPatAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pat"));

		public async Task<string> GetCuddleAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cuddle"));

		public async Task<string> GetLizardAsync() =>
			await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lizard"));

		//randomcat & thecatapi
		public async Task<string> GetKittyAsync()
		{
			try
			{
				var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://aws.random.cat/meow"));
				dynamic item = JsonConvert.DeserializeObject(rawresp);
				var img = item["file"];
				if (img == null) return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
				return img;
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("RandomCat", ex.Message, Discord.LogSeverity.Error, ex));
				try
				{
					var webcli = (HttpWebRequest)WebRequest.Create("http://thecatapi.com/api/images/get");
					webcli.AllowAutoRedirect = true;
					WebResponse resp = await webcli.GetResponseAsync();
					if (resp != null)
					{
						if (resp.ResponseUri != null) return resp.ResponseUri.OriginalString;
						else return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
					}
					else return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
				}
				catch (Exception ex2)
				{
					await logger.AddToLogsAsync(new Models.LogMessage("RandomCat", ex2.Message, Discord.LogSeverity.Error, ex2));
					return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
				}
			}
		}

		//randomdog
		public async Task<string> GetDoggoAsync()
		{
			try
			{
				var resp = await WebHandler.ReturnStringAsync(new Uri("https://random.dog/woof"));
				if (resp == null) return "https://i.imgur.com/ZSMi3Zt.jpg";
				return "https://random.dog/" + resp;
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("RandomDog", ex.Message, Discord.LogSeverity.Error, ex));
				return "https://i.imgur.com/ZSMi3Zt.jpg";
			}
		}
	}
}
