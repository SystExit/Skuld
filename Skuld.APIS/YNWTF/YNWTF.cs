using Newtonsoft.Json;
using Skuld.APIS.YNWTF.Models;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class YNWTFClient
    {
        private readonly RateLimiter rateLimiter;

        public YNWTFClient()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<YNWTFResponce> AskYNWTF()
        {
            if (rateLimiter.IsRatelimited()) return null;

            return JsonConvert.DeserializeObject<YNWTFResponce>(await HttpWebClient.ReturnStringAsync(new Uri($"https://yesno.wtf/api")).ConfigureAwait(false));
        }
    }
}