using Newtonsoft.Json.Linq;
using Skuld.APIS.NekoLife.Models;
using Skuld.APIS.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class NekosLifeClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;

        public NekosLifeClient() : base()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<string> GetAsync(NekoImageType image)
        {
            switch (image)
            {
                case NekoImageType.Neko:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/neko")).ConfigureAwait(false);

                case NekoImageType.LewdNeko:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd")).ConfigureAwait(false);

                case NekoImageType.Hug:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hug")).ConfigureAwait(false);

                case NekoImageType.Pat:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pat")).ConfigureAwait(false);

                case NekoImageType.Cuddle:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cuddle")).ConfigureAwait(false);

                case NekoImageType.Lizard:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lizard")).ConfigureAwait(false);

                case NekoImageType.Feet:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feet")).ConfigureAwait(false);

                case NekoImageType.Yuri:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/yuri")).ConfigureAwait(false);

                case NekoImageType.Trap:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/trap")).ConfigureAwait(false);

                case NekoImageType.Futanari:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/futanari")).ConfigureAwait(false);

                case NekoImageType.HoloLewd:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hololewd")).ConfigureAwait(false);

                case NekoImageType.LewdKemo:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewdkemo")).ConfigureAwait(false);

                case NekoImageType.SoloG:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/solog")).ConfigureAwait(false);

                case NekoImageType.FeetG:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feetg")).ConfigureAwait(false);

                case NekoImageType.Cum:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cum")).ConfigureAwait(false);

                case NekoImageType.EroKemo:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erokemo")).ConfigureAwait(false);

                case NekoImageType.Les:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/les")).ConfigureAwait(false);

                case NekoImageType.Wallpaper:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/wallpaper")).ConfigureAwait(false);

                case NekoImageType.LewdK:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewdk")).ConfigureAwait(false);

                case NekoImageType.Ngif:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/ngif")).ConfigureAwait(false);

                case NekoImageType.Meow:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/meow")).ConfigureAwait(false);

                case NekoImageType.Tickle:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/tickle")).ConfigureAwait(false);

                case NekoImageType.Lewd:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd")).ConfigureAwait(false);

                case NekoImageType.Feed:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feed")).ConfigureAwait(false);

                case NekoImageType.Gecg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/gecg")).ConfigureAwait(false);

                case NekoImageType.Eroyuri:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/eroyuri")).ConfigureAwait(false);

                case NekoImageType.Eron:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/eron")).ConfigureAwait(false);

                case NekoImageType.Cum_jpg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cum_jpg")).ConfigureAwait(false);

                case NekoImageType.BJ:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/bj")).ConfigureAwait(false);

                case NekoImageType.NSFW_Neko_gif:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/nsfw_neko_gif")).ConfigureAwait(false);

                case NekoImageType.EroFeet:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erofeet")).ConfigureAwait(false);

                case NekoImageType.Holo:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/holo")).ConfigureAwait(false);

                case NekoImageType.Keta:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/keta")).ConfigureAwait(false);

                case NekoImageType.Blowjob:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/blowjob")).ConfigureAwait(false);

                case NekoImageType.Pussy:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pussy")).ConfigureAwait(false);

                case NekoImageType.Tits:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/tits")).ConfigureAwait(false);

                case NekoImageType.HoloEro:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/holoero")).ConfigureAwait(false);

                case NekoImageType.pussy_jpg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pussy_jpg")).ConfigureAwait(false);

                case NekoImageType.pwankg:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pwankg")).ConfigureAwait(false);

                case NekoImageType.Classic:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/classic")).ConfigureAwait(false);

                case NekoImageType.Kuni:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kuni")).ConfigureAwait(false);

                case NekoImageType.Waifu:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/waifu")).ConfigureAwait(false);

                case NekoImageType.eightball:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/8ball")).ConfigureAwait(false);

                case NekoImageType.Kiss:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kiss")).ConfigureAwait(false);

                case NekoImageType.Femdom:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/femdom")).ConfigureAwait(false);

                case NekoImageType.Spank:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/spank")).ConfigureAwait(false);

                case NekoImageType.Erok:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erok")).ConfigureAwait(false);

                case NekoImageType.Fox_Girl:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/fox_girl")).ConfigureAwait(false);

                case NekoImageType.Boobs:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/boobs")).ConfigureAwait(false);

                case NekoImageType.Random_hentai_gif:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/random_hentai_gif")).ConfigureAwait(false);

                case NekoImageType.Smallboobs:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/smallboobs")).ConfigureAwait(false);

                case NekoImageType.ero:
                    return await GetImageAsync(new Uri("https://nekos.life/api/v2/img/ero")).ConfigureAwait(false);
            }
            return null;
        }

        private async Task<string> GetImageAsync(Uri uri)
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