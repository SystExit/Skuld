using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skuld.APIS.Utilities;
using Skuld.APIS.WebComics.XKCD.Models;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS.WebComics.XKCD
{
    public class XKCDClient : BaseClient
    {
        private readonly Random random;
        private readonly RateLimiter rateLimiter;
        private int? XKCDLastPage;

        public XKCDClient(Random ran) : base()
        {
            random = ran;
            rateLimiter = new RateLimiter();
            GetLastPageAsync().GetAwaiter().GetResult();
        }

        private async Task<int?> GetLastPageAsync()
        {
            var rawresp = await ReturnStringAsync(new Uri("https://xkcd.com/info.0.json"));
            var jsonresp = JObject.Parse(rawresp);
            dynamic item = jsonresp;
            if (item["num"].ToString() != null)
            {
                int num = Convert.ToInt32(item["num"].ToString());
                return num;
            }
            else
                return null;
        }

        public async Task<XKCDComic> GetRandomComicAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;

            if (!XKCDLastPage.HasValue)
            {
                XKCDLastPage = await GetLastPageAsync();

                return await GetComicAsync(random.Next(0, XKCDLastPage.Value));
            }
            else
            {
                return await GetComicAsync(random.Next(0, XKCDLastPage.Value));
            }
        }

        public async Task<XKCDComic> GetComicAsync(int comicid)
        {
            if (!XKCDLastPage.HasValue)
            {
                XKCDLastPage = await GetLastPageAsync().ConfigureAwait(false);
            }
            if (!rateLimiter.IsRatelimited())
            {
                if (comicid < XKCDLastPage.Value && comicid > 0)
                    return JsonConvert.DeserializeObject<XKCDComic>((await ReturnStringAsync(new Uri($"https://xkcd.com/{comicid}/info.0.json"))));
                else
                    return JsonConvert.DeserializeObject<XKCDComic>((await ReturnStringAsync(new Uri($"https://xkcd.com/{XKCDLastPage.Value}/info.0.json"))));
            }
            else
            {
                Log.Error("XKCDClient", "Ratelimited");
                return null;
            }
        }
    }
}