using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Skuld.Models.API;
using Newtonsoft.Json;

namespace Skuld.APIS
{
    public class XKCDClient
    {
        public static int? XKCDLastPage = GetXKCDLastPage().Result;
        public static async Task<int?> GetXKCDLastPage()
        {
            var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://xkcd.com/info.0.json"));
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
        public static async Task<XKCDComic> GetXKCDComic(int comicid)
        {
            if (XKCDLastPage.HasValue)
			{
				if(comicid<XKCDLastPage.Value&&comicid>0)				
					return JsonConvert.DeserializeObject<XKCDComic>((await WebHandler.ReturnStringAsync(new Uri($"https://xkcd.com/{comicid}/info.0.json"))));				
				else
					return JsonConvert.DeserializeObject<XKCDComic>((await WebHandler.ReturnStringAsync(new Uri($"https://xkcd.com/{XKCDLastPage.Value}/info.0.json"))));
			}                
            else
            {
                await GetXKCDLastPage().ConfigureAwait(false);
				return await GetXKCDComic(comicid).ConfigureAwait(false);
            }
        }
    }
}
