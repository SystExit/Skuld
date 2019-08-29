using Newtonsoft.Json;
using Skuld.APIS.Extensions;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.APIS.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class UrbanDictionaryClient : BaseClient
    {
        private static readonly Uri RandomEndpoint = new Uri("http://api.urbandictionary.com/v0/random");
        private static readonly Uri QueryEndPoint = new Uri("http://api.urbandictionary.com/v0/define?term=");

        private readonly RateLimiter rateLimiter;

        public UrbanDictionaryClient() : base()
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

            return JsonConvert.DeserializeObject<UrbanWordContainer>(raw).List.GetRandomItem();
        }
    }
}