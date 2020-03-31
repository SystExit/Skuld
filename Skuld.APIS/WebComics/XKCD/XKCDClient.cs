using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skuld.APIS.WebComics.XKCD.Models;
using Skuld.Core;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS.WebComics.XKCD
{
    public class XKCDClient
    {
        private readonly RateLimiter rateLimiter;
        private int? XKCDLastPage;

        public XKCDClient()
        {
            rateLimiter = new RateLimiter();
            GetLastPageAsync().GetAwaiter().GetResult();
        }

        private async Task<int?> GetLastPageAsync()
        {
            var rawresp = await HttpWebClient.ReturnStringAsync(new Uri("https://xkcd.com/info.0.json")).ConfigureAwait(false);

            if (string.IsNullOrEmpty(rawresp) || string.IsNullOrWhiteSpace(rawresp)) return null;

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
                XKCDLastPage = await GetLastPageAsync().ConfigureAwait(false);

                return await GetComicAsync(SkuldRandom.Next(0, XKCDLastPage.Value)).ConfigureAwait(false);
            }
            else
            {
                return await GetComicAsync(SkuldRandom.Next(0, XKCDLastPage.Value)).ConfigureAwait(false);
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
                    return JsonConvert.DeserializeObject<XKCDComic>(await HttpWebClient.ReturnStringAsync(new Uri($"https://xkcd.com/{comicid}/info.0.json")).ConfigureAwait(false));
                else
                    return JsonConvert.DeserializeObject<XKCDComic>(await HttpWebClient.ReturnStringAsync(new Uri($"https://xkcd.com/{XKCDLastPage.Value}/info.0.json")).ConfigureAwait(false));
            }
            else
            {
                return null;
            }
        }
    }
}