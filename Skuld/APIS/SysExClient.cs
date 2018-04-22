using System;
using System.Threading.Tasks;
using Skuld.Models.API.SysEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Skuld.APIS
{
    public class SysExClient
    {
		readonly Random random;
		public SysExClient(Random ran)//depinj
		{
			random = ran;
		}

		public async Task<string> GetLlamaAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/llama.json"));
		
		public async Task<string> GetSealAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/seal.json"));
		
		public async Task<string> GetDuckAsync() =>
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/duck.json"));
		
		public async Task<string> GetSquirrelAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/squirrel.json"));
		
		public async Task<string> GetLizardAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/lizard.json"));
		
		public async Task<string> GetMorphAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/morphs.json"));
		
		public async Task<string> GetSnakeAsync() =>
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/snake.json"));

		async Task<string> GetAnimalAsync(Uri url)
		{
			var resp = await WebHandler.ReturnStringAsync(url);
			var items = JsonConvert.DeserializeObject<List<Animal>>(resp);
			if (items == null) return null;
			var animal = items[random.Next(0,items.Count)].URL;
			return animal;
		}

		public async Task<string> GetRoastAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/roasts.json"));
			var items = JsonConvert.DeserializeObject<List<Roasts>>(resp);
			if (items == null) return null;
			return items[random.Next(0, items.Count)].Roast;
		}

		public async Task<Joke> GetDadJokeAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/dadjokes.json"));
			var items = JsonConvert.DeserializeObject<List<Joke>>(resp);
			if (items == null) return null;
			return items[random.Next(0, items.Count)];
		}

		public async Task<Joke> GetPickupLineAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/pickuplines.json"));
			var items = JsonConvert.DeserializeObject<List<Joke>>(resp);
			if (items == null) return null;
			return items[random.Next(0, items.Count)];
		}

		public async Task<string> GetWeebActionGifAsync(GifType type)
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/actions.json"));
			var items = JsonConvert.DeserializeObject<List<WeebGif>>(resp);
			if (items == null) return null;
			var typed = items.Where(x => x.GifType == type).ToList();
			return typed[random.Next(0, typed.Count)].URL;
		}

		public async Task<List<WeebGif>> GetAllWeebActionGifsAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/actions.json"));
			if (resp == null) return null;
			return JsonConvert.DeserializeObject<List<WeebGif>>(resp);
		}

		public async Task<string> GetWeebReactionGifAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/reactions.json"));
			var items = JsonConvert.DeserializeObject<List<WeebGif>>(resp);
			if (items == null) return null;
			return items[random.Next(0, items.Count)].URL;
		}

		public async Task<string> GetLewdKitsuneAsync()
		{
			var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://kitsu.systemexit.co.uk/lewd"));
			dynamic item = JObject.Parse(rawresp);
			var img = item["kitsune"];
			if (img == null) return null;
			return img;
		}
		public async Task<string> GetKitsuneAsync()
		{
			var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://kitsu.systemexit.co.uk/kitsune"));
			dynamic item = JObject.Parse(rawresp);
			var img = item["kitsune"];
			if (img == null) return null;
			return img;
		}
	}
}
