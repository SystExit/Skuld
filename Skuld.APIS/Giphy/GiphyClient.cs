using Newtonsoft.Json.Linq;
using Skuld.APIS.Giphy.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Services;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class GiphyClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;
        private readonly Random random;

        public GiphyClient(GenericLogger log) : base(log)
        {
            rateLimiter = new RateLimiter();
            random = new Random();
        }

        public async Task<GiphyGif> GetGifAsync(string query)
        {
            if (rateLimiter.IsRatelimited()) return null;

            query = query.Replace(" ", "%20");
            var rawresp = await ReturnStringAsync(new Uri($"https://api.giphy.com/v1/gifs/search?q={query}&api_key=dc6zaTOxFJmzC"));
            var jsonresp = JObject.Parse(rawresp);
            var photo = (JArray)jsonresp["data"];
            dynamic item = photo[random.Next(0, photo.Count)];
            return new GiphyGif
            {
                ID = item["id"].ToString()
            };
        }
    }
}