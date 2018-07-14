using Newtonsoft.Json.Linq;
using Skuld.APIS.NekoLife.Models;
using System;
using System.Threading.Tasks;
using Skuld.APIS.Utilities;
using Skuld.Core.Services;

namespace Skuld.APIS
{
    public class NekosLifeClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;

        public NekosLifeClient(GenericLogger log) : base(log)
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<string> GetAsync(NekoImageType image)
        {
            switch (image)
            {
                case NekoImageType.Neko:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/neko"));

                case NekoImageType.LewdNeko:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd"));

                case NekoImageType.Hug:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hug"));

                case NekoImageType.Pat:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pat"));

                case NekoImageType.Cuddle:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cuddle"));

                case NekoImageType.Lizard:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lizard"));

                case NekoImageType.Feet:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feet"));

                case NekoImageType.Yuri:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/yuri"));

                case NekoImageType.Trap:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/trap"));

                case NekoImageType.Futanari:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/futanari"));

                case NekoImageType.HoloLewd:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hololewd"));

                case NekoImageType.LewdKemo:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewdkemo"));

                case NekoImageType.SoloG:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/solog"));

                case NekoImageType.FeetG:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feetg"));

                case NekoImageType.Cum:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cum"));

                case NekoImageType.EroKemo:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erokemo"));

                case NekoImageType.Les:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/les"));

                case NekoImageType.Wallpaper:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/wallpaper"));

                case NekoImageType.LewdK:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewdk"));

                case NekoImageType.Ngif:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/ngif"));

                case NekoImageType.Meow:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/meow"));

                case NekoImageType.Tickle:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/tickle"));

                case NekoImageType.Lewd:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd"));

                case NekoImageType.Feed:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feed"));

                case NekoImageType.Gecg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/gecg"));

                case NekoImageType.Eroyuri:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/eroyuri"));

                case NekoImageType.Eron:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/eron"));

                case NekoImageType.Cum_jpg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cum_jpg"));

                case NekoImageType.BJ:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/bj"));

                case NekoImageType.NSFW_Neko_gif:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/nsfw_neko_gif"));

                case NekoImageType.EroFeet:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erofeet"));

                case NekoImageType.Holo:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/holo"));

                case NekoImageType.Keta:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/keta"));

                case NekoImageType.Blowjob:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/blowjob"));

                case NekoImageType.Pussy:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pussy"));

                case NekoImageType.Tits:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/tits"));

                case NekoImageType.HoloEro:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/holoero"));

                case NekoImageType.pussy_jpg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pussy_jpg"));

                case NekoImageType.pwankg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pwankg"));

                case NekoImageType.Classic:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/classic"));

                case NekoImageType.Kuni:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kuni"));

                case NekoImageType.Waifu:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/waifu"));

                case NekoImageType.eightball:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/8ball"));

                case NekoImageType.Kiss:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kiss"));

                case NekoImageType.Femdom:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/femdom"));

                case NekoImageType.Spank:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/spank"));

                case NekoImageType.Erok:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erok"));

                case NekoImageType.Fox_Girl:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/fox_girl"));

                case NekoImageType.Boobs:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/boobs"));

                case NekoImageType.Random_hentai_gif:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/random_hentai_gif"));

                case NekoImageType.Smallboobs:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/smallboobs"));

                case NekoImageType.ero:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/ero"));
            }
            return null;
        }

        async Task<string> GetImageAsync(Uri uri)
        {
            if (rateLimiter.IsRatelimited()) return null;

            var rawresp = await ReturnStringAsync(uri);
            dynamic item = JObject.Parse(rawresp);
            var img = item["url"];
            if (img == null) return null;
            return img;
        }

        public async Task<string> GetFactAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;

            var rawresp = await ReturnStringAsync(new Uri("https://nekos.life/api/v2/fact"));
            dynamic item = JObject.Parse(rawresp);
            var fact = item["fact"];
            if (fact == null) return null;
            return fact;
        }
    }
}