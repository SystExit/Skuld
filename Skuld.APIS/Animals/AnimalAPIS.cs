using Newtonsoft.Json;
using Skuld.APIS.Animals.Models;
using Skuld.Core.Utilities;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class AnimalClient
    {
        private readonly Uri BIRDAPI = new Uri("https://random.birb.pw/tweet.json/");
        private readonly Uri KITTYAPI = new Uri("https://aws.random.cat/meow");
        private readonly Uri KITTYAPI2 = new Uri("http://thecatapi.com/api/images/get");
        private readonly Uri DOGGOAPI = new Uri("https://random.dog/woof");
        private readonly RateLimiter cat1rateLimiter;
        private readonly RateLimiter cat2rateLimiter;
        private readonly RateLimiter birdrateLimiter;
        private readonly RateLimiter dograteLimiter;

        public AnimalClient()
        {
            cat1rateLimiter = new RateLimiter();
            cat2rateLimiter = new RateLimiter();
            birdrateLimiter = new RateLimiter();
            dograteLimiter = new RateLimiter();
        }

        public async Task<string> GetAnimalAsync(AnimalType type)
            => type switch
            {
                AnimalType.Bird => await GetBirdAsync().ConfigureAwait(false),
                AnimalType.Doggo => await GetDoggoAsync().ConfigureAwait(false),
                AnimalType.Kitty => await GetKittyAsync().ConfigureAwait(false),
                _ => null
            };

        private async Task<string> GetBirdAsync()
        {
            if (birdrateLimiter.IsRatelimited())
            { 
                return null; 
            }

            var rawresp = await HttpWebClient.ReturnStringAsync(BIRDAPI).ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(rawresp);
            var birb = data["file"];
            if (birb == null)
            {
                return null;
            }
            return "https://random.birb.pw/img/" + birb;
        }

        private async Task<string> GetKittyAsync()
        {
            try
            {
                if (cat1rateLimiter.IsRatelimited())
                {
                    return null;
                }

                var rawresp = await HttpWebClient.ReturnStringAsync(KITTYAPI).ConfigureAwait(false);
                dynamic item = JsonConvert.DeserializeObject(rawresp);
                var img = item["file"];
                if (img == null) return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                return img;
            }
            catch
            {
                try
                {
                    if (cat2rateLimiter.IsRatelimited())
                    {
                        return null;
                    }

                    var webcli = (HttpWebRequest)WebRequest.Create(KITTYAPI2);
                    webcli.Headers.Add(HttpRequestHeader.UserAgent, HttpWebClient.UAGENT);
                    webcli.AllowAutoRedirect = true;
                    WebResponse resp = await webcli.GetResponseAsync().ConfigureAwait(false);
                    if (resp != null)
                    {
                        if (resp.ResponseUri != null)
                        {
                            return resp.ResponseUri.OriginalString;
                        }
                        else
                        {
                            return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                        }
                    }
                    else
                    {
                        return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                    }
                }
                catch
                {
                    return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                }
            }
        }

        private async Task<string> GetDoggoAsync()
        {
            try
            {
                if (dograteLimiter.IsRatelimited()) 
                {
                    return null;
                }

                var resp = await HttpWebClient.ReturnStringAsync(DOGGOAPI).ConfigureAwait(false);
                if (resp == null)
                {
                    return "https://i.imgur.com/ZSMi3Zt.jpg";
                }
                return "https://random.dog/" + resp;
            }
            catch
            {
                return "https://i.imgur.com/ZSMi3Zt.jpg";
            }
        }
    }
}