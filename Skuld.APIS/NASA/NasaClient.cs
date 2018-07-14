using Newtonsoft.Json;
using Skuld.APIS.NASA.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Services;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class NASAClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;
        private string token;

        public NASAClient(GenericLogger log, string tok) : base(log)
        {
            rateLimiter = new RateLimiter();
            token = tok;
        }

        public async Task<APOD> GetAPODAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;

            var client = (HttpWebRequest)WebRequest.Create("https://api.nasa.gov/planetary/apod?api_key=" + token);
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            var response = (HttpWebResponse)await client.GetResponseAsync();
            int remainingcalls = 0;

            for (int x = 0; x < response.Headers.Count; x++)
            {
                if (response.Headers.Keys[x] == "X-RateLimit-Remaining")
                {
                    remainingcalls = Convert.ToInt32(response.Headers[x]);
                    break;
                }
            }

            if (remainingcalls >= 0)
            {
                var streamresp = response.GetResponseStream();
                var sr = new StreamReader(streamresp);
                var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<APOD>(stringifiedresp);
            }
            else
            {
                return null;
            }
        }
    }
}