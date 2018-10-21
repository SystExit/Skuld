using Newtonsoft.Json;
using Skuld.APIS.ISS.Models;
using Skuld.APIS.Utilities;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class ISSClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;
        private string AstrosEndpoint = "http://api.open-notify.org/astros.json";
        private string PositionEndpoint = "http://api.open-notify.org/iss-now.json";

        public ISSClient() : base()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<Astros> GetAstronautsInSpace()
        {
            if (rateLimiter.IsRatelimited()) return null;

            var client = (HttpWebRequest)WebRequest.Create(AstrosEndpoint);
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            var response = (HttpWebResponse)await client.GetResponseAsync();

            var streamresp = response.GetResponseStream();
            var sr = new StreamReader(streamresp);
            var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<Astros>(stringifiedresp);
        }

        public async Task<ISSPosition> GetISSPositionAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;

            var client = (HttpWebRequest)WebRequest.Create(PositionEndpoint);
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            var response = (HttpWebResponse)await client.GetResponseAsync();

            var streamresp = response.GetResponseStream();
            var sr = new StreamReader(streamresp);
            var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ISSPosition>(stringifiedresp);
        }
    }
}
