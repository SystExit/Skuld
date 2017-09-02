using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Skuld.Models.API;
using Newtonsoft.Json;

namespace Skuld.APIS
{
    public partial class APIWebReq
    {
        public static int? XKCDLastPage = GetXKCDLastPage().Result;
        public static async Task<int?> GetXKCDLastPage()
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
        public static async Task<XKCDComic> GetXKCDComic(int comicid)
        {
            if (XKCDLastPage.HasValue)
                return JsonConvert.DeserializeObject<XKCDComic>((await APIWebReq.ReturnString(new Uri($"https://xkcd.com/{comicid}/info.0.json"))));
            else
            {
                await GetXKCDLastPage();
                return null;
            }
        }
    }
}
