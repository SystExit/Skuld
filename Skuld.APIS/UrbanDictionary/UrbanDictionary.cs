using Newtonsoft.Json;
using Skuld.APIS.Utilities;
using System;
using System.Threading.Tasks;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.Core.Services;

namespace Skuld.APIS
{
    public class UrbanDictionaryClient : BaseClient
    {
        private static Uri RandomEndpoint = new Uri("http://api.urbandictionary.com/v0/random");
        private static Uri QueryEndPoint = new Uri("http://api.urbandictionary.com/v0/define?term=");

        private readonly RateLimiter rateLimiter;

        public UrbanDictionaryClient(GenericLogger log) : base(log)
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<UrbanWord> GetRandomWordAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;

            var raw = await ReturnStringAsync(RandomEndpoint);
            return JsonConvert.DeserializeObject<UrbanWord>(raw);
        }

        public async Task<UrbanWord> GetPhraseAsync(string phrase)
        {
            if (rateLimiter.IsRatelimited()) return null;

            var raw = await ReturnStringAsync(new Uri(QueryEndPoint + phrase));
            return JsonConvert.DeserializeObject<UrbanWord>(raw);
        }
    }
}