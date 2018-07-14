using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Skuld.APIS.Animals.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Services;

namespace Skuld.APIS
{
    public class AnimalClient : BaseClient
    {
        private readonly Uri BIRDAPI = new Uri("https://birdsare.cool/bird.json");
        private readonly Uri KITTYAPI = new Uri("https://aws.random.cat/meow");
        private readonly Uri KITTYAPI2 = new Uri("http://thecatapi.com/api/images/get");
        private readonly Uri DOGGOAPI = new Uri("https://random.dog/woof");
        private readonly RateLimiter cat1rateLimiter;
        private readonly RateLimiter cat2rateLimiter;
        private readonly RateLimiter birdrateLimiter;
        private readonly RateLimiter dograteLimiter;

        public AnimalClient(GenericLogger log) : base(log)
        {
            cat1rateLimiter = new RateLimiter();
            cat2rateLimiter = new RateLimiter();
            birdrateLimiter = new RateLimiter();
            dograteLimiter = new RateLimiter();
        }

        public async Task<string> GetAnimalAsync(AnimalType type)
        {
            switch (type)
            {
                case AnimalType.Bird:
                    return await GetBirdAsync();
                case AnimalType.Doggo:
                    return await GetDoggoAsync();
                case AnimalType.Kitty:
                    return await GetKittyAsync();
            }

            return null;
        }

        async Task<string> GetBirdAsync()
        {
            if (birdrateLimiter.IsRatelimited()) return null;

            var rawresp = await ReturnStringAsync(BIRDAPI);
            dynamic data = JsonConvert.DeserializeObject(rawresp);
            var birb = data["url"];
            if (birb == null) return null;
            return birb;
        }

        async Task<string> GetKittyAsync()
        {
            try
            {
                if (cat1rateLimiter.IsRatelimited()) return null;

                var rawresp = await ReturnStringAsync(KITTYAPI);
                dynamic item = JsonConvert.DeserializeObject(rawresp);
                var img = item["file"];
                if (img == null) return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                return img;
            }
            catch (Exception ex)
            {
                await loggingService.AddToLogsAsync(new Core.Models.LogMessage("RandomCat", ex.Message, Discord.LogSeverity.Error, ex));
                try
                {
                    if (cat2rateLimiter.IsRatelimited()) return null;

                    var webcli = (HttpWebRequest)WebRequest.Create(KITTYAPI2);
                    webcli.Headers.Add(HttpRequestHeader.UserAgent, UAGENT);
                    webcli.AllowAutoRedirect = true;
                    WebResponse resp = await webcli.GetResponseAsync();
                    if (resp != null)
                    {
                        if (resp.ResponseUri != null) return resp.ResponseUri.OriginalString;
                        else return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                    }
                    else return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                }
                catch (Exception ex2)
                {
                    await loggingService.AddToLogsAsync(new Core.Models.LogMessage("RandomCat", ex2.Message, Discord.LogSeverity.Error, ex2));
                    return "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg";
                }
            }
        }

        async Task<string> GetDoggoAsync()
        {
            try
            {
                if (dograteLimiter.IsRatelimited()) return null;

                var resp = await ReturnStringAsync(DOGGOAPI);
                if (resp == null) return "https://i.imgur.com/ZSMi3Zt.jpg";
                return "https://random.dog/" + resp;
            }
            catch (Exception ex)
            {
                await loggingService.AddToLogsAsync(new Core.Models.LogMessage("RandomDog", ex.Message, Discord.LogSeverity.Error, ex));
                return "https://i.imgur.com/ZSMi3Zt.jpg";
            }
        }
    }
}