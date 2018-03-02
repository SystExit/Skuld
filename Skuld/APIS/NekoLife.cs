using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Skuld.APIS
{
    public class NekoLife
    {
        public static async Task<string> GetNekoAsync()
        {
            var rawresp = await APIWebReq.ReturnString(new Uri("https://nekos.life/api/neko"));
            var jsonresp = JObject.Parse(rawresp);
            dynamic item = jsonresp;
            if (item["neko"] != null)
                return item["neko"];
            return null;
        }
        public static async Task<string> GetLewdNekoAsync()
        {
            var rawresp = await APIWebReq.ReturnString(new Uri("https://nekos.life/api/lewd/neko"));
            var jsonresp = JObject.Parse(rawresp);
            dynamic item = jsonresp;
            if (item["neko"] != null)
                return item["neko"];
            return null;
        }
    }
}
