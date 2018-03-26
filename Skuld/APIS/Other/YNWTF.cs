using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skuld.APIS
{
    public class YNWTF
    {
        public static async Task<Models.API.YNWTF> AskYNWTF()
            => JsonConvert.DeserializeObject<Models.API.YNWTF>((await WebHandler.ReturnStringAsync(new Uri($"https://yesno.wtf/api"))));
    }
}
