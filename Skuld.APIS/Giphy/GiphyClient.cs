using Newtonsoft.Json.Linq;
using Skuld.APIS.Giphy.Models;
using Skuld.Core.Extensions;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
	public class GiphyClient
	{
		private readonly string apiKey;
		private readonly RateLimiter rateLimiter;

		public GiphyClient(string apiKey)
		{
			this.apiKey = apiKey;
			rateLimiter = new RateLimiter();
		}

		public async Task<GiphyGif> GetGifAsync(string query)
		{
			if (apiKey is null)
			{
				return null;
			}

			if (query is null)
			{
				return null;
			}

			if (rateLimiter.IsRatelimited())
			{
				return null;
			}

			var rawresp = await HttpWebClient.ReturnStringAsync(new Uri($"https://api.giphy.com/v1/gifs/search?q={query.Replace(" ", "%20")}&api_key={apiKey}")).ConfigureAwait(false);

			var jsonresp = JObject.Parse(rawresp);
			var photo = (JArray)jsonresp["data"];

			if (photo is not null)
			{
				dynamic item = photo.CryptoRandom();

				return new GiphyGif
				{
					ID = item["id"].ToString()
				};
			}

			return null;
		}
	}
}