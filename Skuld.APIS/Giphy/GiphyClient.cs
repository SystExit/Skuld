using Newtonsoft.Json.Linq;
using Skuld.APIS.Giphy.Models;
using Skuld.Core;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class GiphyClient
    {
        private readonly RateLimiter rateLimiter;

        public GiphyClient()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<GiphyGif> GetGifAsync(string query)
        {
            if (rateLimiter.IsRatelimited()) return null;

            query = query.Replace(" ", "%20");
            var rawresp = await HttpWebClient.ReturnStringAsync(new Uri($"https://api.giphy.com/v1/gifs/search?q={query}&api_key=dc6zaTOxFJmzC")).ConfigureAwait(false);
            var jsonresp = JObject.Parse(rawresp);
            var photo = (JArray)jsonresp["data"];
            dynamic item = photo[SkuldRandom.Next(0, photo.Count)];
            return new GiphyGif
            {
                ID = item["id"].ToString()
            };
        }
    }
}