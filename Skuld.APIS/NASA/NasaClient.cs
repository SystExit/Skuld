using Newtonsoft.Json;
using Skuld.APIS.NASA.Models;
using Skuld.Core.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Skuld.APIS
{
	public class NASAClient
	{
		private readonly RateLimiter rateLimiter;
		private readonly string token;

		public NASAClient(string tok)
		{
			rateLimiter = new RateLimiter();
			token = tok;
		}

		public async Task<APOD> GetAPODAsync()
		{
			if (token is null) return null;
			if (rateLimiter.IsRatelimited()) return null;

			var client = (HttpWebRequest)WebRequest.Create("https://api.nasa.gov/planetary/apod?api_key=" + token);
			client.Headers.Add(HttpRequestHeader.UserAgent, HttpWebClient.UAGENT);

			var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false);
			int remainingcalls = 0;

			for (int x = 0; x < response.Headers.Count; x++)
			{
				if (response.Headers.Keys[x] == "X-RateLimit-Remaining")
				{
					remainingcalls = Convert.ToInt32(response.Headers[x], CultureInfo.InvariantCulture);
					break;
				}
			}

			if (remainingcalls >= 0)
			{
				var streamresp = response.GetResponseStream();
				using var sr = new StreamReader(streamresp);
				var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<APOD>(stringifiedresp);
			}
			else
			{
				return null;
			}
		}
	}
}