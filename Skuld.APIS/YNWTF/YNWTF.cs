using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Skuld.APIS.YNWTF.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Services;

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