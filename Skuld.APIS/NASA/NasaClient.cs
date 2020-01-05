using Newtonsoft.Json;
using Skuld.APIS.NASA.Models;
using Skuld.APIS.Utilities;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class NASAClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;
        private readonly string token;

        private readonly string CuriosityEndpoint = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity";
        private readonly string OpportunityEndpoint = "https://api.nasa.gov/mars-photos/api/v1/rovers/opportunity";
        private readonly string SpiritEndpoint = "https://api.nasa.gov/mars-photos/api/v1/rovers/spirit";

        private int CuriosityMaxSOL = 0;
        private int OpportunityMaxSOL = 0;
        private int SpiritMaxSOL = 0;

        public NASAClient(string tok) : base()
        {
            rateLimiter = new RateLimiter();
            token = tok;
            FeedSOL();
        }

        private async void FeedSOL()
        {
            if (token == null) return;

            //Curiosity
            var client = (HttpWebRequest)WebRequest.Create(CuriosityEndpoint + $"?api_key={token}");
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false);

            var streamresp = response.GetResponseStream();

            using (var sr = new StreamReader(streamresp))
            {
                var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<RoverWrapper>(stringifiedresp);

                CuriosityMaxSOL = data.Rover.MaxSOL;
            }

            //Opportunity
            client = (HttpWebRequest)WebRequest.Create(OpportunityEndpoint + $"?api_key={token}");
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false);

            streamresp = response.GetResponseStream();

            using (var sr = new StreamReader(streamresp))
            {
                var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<RoverWrapper>(stringifiedresp);

                OpportunityMaxSOL = data.Rover.MaxSOL;
            }

            //Spirit
            client = (HttpWebRequest)WebRequest.Create(SpiritEndpoint + $"?api_key={token}");
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false);

            streamresp = response.GetResponseStream();
            using (var sr = new StreamReader(streamresp))
            {
                var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<RoverWrapper>(stringifiedresp);

                SpiritMaxSOL = data.Rover.MaxSOL;
            }
        }

        public async Task<APOD> GetAPODAsync()
        {
            if (token == null) return null;
            if (rateLimiter.IsRatelimited()) return null;

            var client = (HttpWebRequest)WebRequest.Create("https://api.nasa.gov/planetary/apod?api_key=" + token);
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false);
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
                using var sr = new StreamReader(streamresp);
                var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<APOD>(stringifiedresp);
            }
            else
            {
                return null;
            }
        }

        public async Task<RoverPhotoWrapper> GetRoverPhotoAsync(NasaRover rover, NasaRoverCamera camera, int SOL = 2199)
        {
            if (token == null) return null;
            if (rateLimiter.IsRatelimited()) return null;

            string requestbase = "";
            switch (rover)
            {
                case NasaRover.Curiosity:
                    if (SOL > CuriosityMaxSOL) return null;
                    requestbase = CuriosityEndpoint;
                    break;

                case NasaRover.Opportunity:
                    if (SOL > OpportunityMaxSOL) return null;
                    requestbase = OpportunityEndpoint;
                    break;

                case NasaRover.Spirit:
                    if (SOL > SpiritMaxSOL) return null;
                    requestbase = SpiritEndpoint;
                    break;
            }

            string request = requestbase += $"/photos?sol={SOL}&camera={camera.ToString().ToLowerInvariant()}&api_key={token}";

            var client = (HttpWebRequest)WebRequest.Create(request);
            client.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);

            var response = (HttpWebResponse)await client.GetResponseAsync().ConfigureAwait(false);
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
                using var sr = new StreamReader(streamresp);
                var stringifiedresp = await sr.ReadToEndAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<RoverPhotoWrapper>(stringifiedresp);

                if (data != null)
                {
                    switch (rover)
                    {
                        case NasaRover.Curiosity:
                            CuriosityMaxSOL = data.Photos[0].Rover.MaxSOL;
                            break;

                        case NasaRover.Opportunity:
                            OpportunityMaxSOL = data.Photos[0].Rover.MaxSOL;
                            break;

                        case NasaRover.Spirit:
                            SpiritMaxSOL = data.Photos[0].Rover.MaxSOL;
                            break;
                    }
                    return data;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}