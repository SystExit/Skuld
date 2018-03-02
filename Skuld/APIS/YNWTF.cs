using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Skuld.Models.API;

namespace Skuld.APIS
{
    public partial class APIWebReq
    {
        public static async Task<YNWTF> AskYNWTF()
            => JsonConvert.DeserializeObject<YNWTF>((await APIWebReq.ReturnString(new Uri($"https://yesno.wtf/api"))));
    }
}
