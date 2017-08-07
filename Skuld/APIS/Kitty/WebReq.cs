using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Skuld.APIS.Kitty
{
    public class WebReq
    {
        public async static Task<Models.API.Kitty> GetKitty()
        {            
            JObject jsonresp = JObject.Parse(await APIWebReq.ReturnString(new Uri("http://random.cat/meow")));
            dynamic item = jsonresp["file"];
            return new Models.API.Kitty(Convert.ToString(item));
        }
    }
}
