using Newtonsoft.Json;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skuld.APIS
{
	public class UrbanDictionaryClient
	{
		private static readonly Uri RandomEndpoint = new("http://api.urbandictionary.com/v0/random");
		private static readonly Uri QueryEndPoint = new("http://api.urbandictionary.com/v0/define?term=");

		private readonly RateLimiter rateLimiter;

		public UrbanDictionaryClient()
		{
			rateLimiter = new RateLimiter();
		}

		public async Task<UrbanWord> GetRandomWordAsync()
		{
			if (rateLimiter.IsRatelimited()) return null;

			var raw = await HttpWebClient.ReturnStringAsync(RandomEndpoint).ConfigureAwait(false);
			return JsonConvert.DeserializeObject<UrbanWord>(raw);
		}

		public async Task<IEnumerable<UrbanWord>> GetPhrasesAsync(string phrase)
		{
			if (rateLimiter.IsRatelimited()) return null;

			var raw = await HttpWebClient.ReturnStringAsync(new Uri(QueryEndPoint + phrase)).ConfigureAwait(false);
			return JsonConvert.DeserializeObject<UrbanWordContainer>(raw).List;
		}
	}
}