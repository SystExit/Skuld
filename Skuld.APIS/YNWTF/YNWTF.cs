using Newtonsoft.Json;
using Skuld.APIS.Utilities;
using Skuld.APIS.YNWTF.Models;
using Skuld.Core.Services;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class YNWTFClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;

        public YNWTFClient(GenericLogger log) : base(log)
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<YNWTFResponce> AskYNWTF()
        {
            if (rateLimiter.IsRatelimited()) return null;

            return JsonConvert.DeserializeObject<YNWTFResponce>((await ReturnStringAsync(new Uri($"https://yesno.wtf/api"))));
        }
    }
}