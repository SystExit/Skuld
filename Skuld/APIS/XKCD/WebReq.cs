using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Skuld.Models.API;
using Newtonsoft.Json;

namespace Skuld.APIS.XKCD
{
    public class WebReq
    {
        public static int? LastPage = GetLastPage().Result;
        public static async Task<int?> GetLastPage()
        {
            var rawresp = await APIWebReq.ReturnString(new Uri("https://xkcd.com/info.0.json"));
            JObject jsonresp = JObject.Parse(rawresp);
            dynamic item = jsonresp;
            if (item["num"].ToString() != null)
            {
                int num = Convert.ToInt32(item["num"].ToString());
                return num;
            }
            else
                return null;
        }
        public static async Task<XKCDComic> GetComic(int comicid)
        {
            if (LastPage.HasValue)
                return JsonConvert.DeserializeObject<XKCDComic>((await APIWebReq.ReturnString(new Uri($"https://xkcd.com/{comicid}/info.0.json"))));
            else
            {
                await GetLastPage();
                return null;
            }
        }
    }
}
