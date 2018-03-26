using System;
using System.Threading.Tasks;
using Skuld.Models.API.SysEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Skuld.APIS
{
    public class SysExClient
    {
		public static async Task<string> GetLlamaAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/llama.json"));
		
		public static async Task<string> GetSealAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/seal.json"));
		
		public static async Task<string> GetDuckAsync() =>
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/duck.json"));
		
		public static async Task<string> GetSquirrelAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/squirrel.json"));
		
		public static async Task<string> GetLizardAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/lizard.json"));
		
		public static async Task<string> GetMorphAsync() =>		
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/morphs.json"));
		
		public static async Task<string> GetSnakeAsync() =>
			await GetAnimalAsync(new Uri("https://api.systemexit.co.uk/get/snake.json"));

		static async Task<string> GetAnimalAsync(Uri url)
		{
			var resp = await WebHandler.ReturnStringAsync(url);
			var items = JsonConvert.DeserializeObject<List<Animal>>(resp);
			if (items == null) return null;
			var animal = items[Bot.random.Next(0,items.Count)].URL;
			return animal;
		}

		public static async Task<string> GetRoastAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/roasts.json"));
			var items = JsonConvert.DeserializeObject<List<Roasts>>(resp);
			if (items == null) return null;
			return items[Bot.random.Next(0, items.Count)].Roast;
		}

		public static async Task<Joke> GetDadJokeAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/dadjokes.json"));
			var items = JsonConvert.DeserializeObject<List<Joke>>(resp);
			if (items == null) return null;
			return items[Bot.random.Next(0, items.Count)];
		}

		public static async Task<Joke> GetPickupLineAsync()
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/pickuplines.json"));
			var items = JsonConvert.DeserializeObject<List<Joke>>(resp);
			if (items == null) return null;
			return items[Bot.random.Next(0, items.Count)];
		}

		public static async Task<string> GetWeebGifAsync(Models.API.SysEx.Type type)
		{
			var resp = await WebHandler.ReturnStringAsync(new Uri("https://api.systemexit.co.uk/get/actions.json"));
			var items = JsonConvert.DeserializeObject<List<WeebGif>>(resp);
			if (items == null) return null;
			var typed = items.Where(x => x.GifType == type).ToList();
			return typed[Bot.random.Next(0, items.Count)].URL;
		}
	}
}
