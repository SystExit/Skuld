using Newtonsoft.Json.Linq;
using Skuld.APIS.NekoLife.Models;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class NekosLifeClient
    {
        private readonly RateLimiter rateLimiter;

        public NekosLifeClient()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<string> GetAsync(NekoImageType image)
        {
            return image switch
            {
                NekoImageType.Neko => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/neko")).ConfigureAwait(false),
                NekoImageType.LewdNeko => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd")).ConfigureAwait(false),
                NekoImageType.Hug => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hug")).ConfigureAwait(false),
                NekoImageType.Pat => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pat")).ConfigureAwait(false),
                NekoImageType.Cuddle => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cuddle")).ConfigureAwait(false),
                NekoImageType.Lizard => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lizard")).ConfigureAwait(false),
                NekoImageType.Feet => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feet")).ConfigureAwait(false),
                NekoImageType.Yuri => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/yuri")).ConfigureAwait(false),
                NekoImageType.Trap => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/trap")).ConfigureAwait(false),
                NekoImageType.Futanari => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/futanari")).ConfigureAwait(false),
                NekoImageType.HoloLewd => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/hololewd")).ConfigureAwait(false),
                NekoImageType.LewdKemo => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewdkemo")).ConfigureAwait(false),
                NekoImageType.SoloG => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/solog")).ConfigureAwait(false),
                NekoImageType.FeetG => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feetg")).ConfigureAwait(false),
                NekoImageType.Cum => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cum")).ConfigureAwait(false),
                NekoImageType.EroKemo => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erokemo")).ConfigureAwait(false),
                NekoImageType.Les => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/les")).ConfigureAwait(false),
                NekoImageType.Wallpaper => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/wallpaper")).ConfigureAwait(false),
                NekoImageType.LewdK => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewdk")).ConfigureAwait(false),
                NekoImageType.Ngif => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/ngif")).ConfigureAwait(false),
                NekoImageType.Meow => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/meow")).ConfigureAwait(false),
                NekoImageType.Tickle => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/tickle")).ConfigureAwait(false),
                NekoImageType.Lewd => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/lewd")).ConfigureAwait(false),
                NekoImageType.Feed => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/feed")).ConfigureAwait(false),
                NekoImageType.Gecg => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/gecg")).ConfigureAwait(false),
                NekoImageType.Eroyuri => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/eroyuri")).ConfigureAwait(false),
                NekoImageType.Eron => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/eron")).ConfigureAwait(false),
                NekoImageType.Cum_jpg => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/cum_jpg")).ConfigureAwait(false),
                NekoImageType.BJ => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/bj")).ConfigureAwait(false),
                NekoImageType.NSFW_Neko_gif => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/nsfw_neko_gif")).ConfigureAwait(false),
                NekoImageType.EroFeet => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erofeet")).ConfigureAwait(false),
                NekoImageType.Holo => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/holo")).ConfigureAwait(false),
                NekoImageType.Keta => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/keta")).ConfigureAwait(false),
                NekoImageType.Blowjob => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/blowjob")).ConfigureAwait(false),
                NekoImageType.Pussy => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pussy")).ConfigureAwait(false),
                NekoImageType.Tits => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/tits")).ConfigureAwait(false),
                NekoImageType.HoloEro => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/holoero")).ConfigureAwait(false),
                NekoImageType.pussy_jpg => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pussy_jpg")).ConfigureAwait(false),
                NekoImageType.pwankg => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/pwankg")).ConfigureAwait(false),
                NekoImageType.Classic => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/classic")).ConfigureAwait(false),
                NekoImageType.Kuni => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kuni")).ConfigureAwait(false),
                NekoImageType.Waifu => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/waifu")).ConfigureAwait(false),
                NekoImageType.eightball => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/8ball")).ConfigureAwait(false),
                NekoImageType.Kiss => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/kiss")).ConfigureAwait(false),
                NekoImageType.Femdom => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/femdom")).ConfigureAwait(false),
                NekoImageType.Spank => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/spank")).ConfigureAwait(false),
                NekoImageType.Erok => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/erok")).ConfigureAwait(false),
                NekoImageType.Fox_Girl => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/fox_girl")).ConfigureAwait(false),
                NekoImageType.Boobs => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/boobs")).ConfigureAwait(false),
                NekoImageType.Random_hentai_gif => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/random_hentai_gif")).ConfigureAwait(false),
                NekoImageType.Smallboobs => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/smallboobs")).ConfigureAwait(false),
                NekoImageType.ero => await GetImageAsync(new Uri("https://nekos.life/api/v2/img/ero")).ConfigureAwait(false),
                _ => null,
            };
        }

        private async Task<string> GetImageAsync(Uri uri)
        {
            if (rateLimiter.IsRatelimited()) return null;

            var rawresp = await HttpWebClient.ReturnStringAsync(uri).ConfigureAwait(false);
            dynamic item = JObject.Parse(rawresp);
            var img = item["url"];
            if (img is null) return null;
            return img;
        }

        public async Task<string> GetFactAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;

            var rawresp = await HttpWebClient.ReturnStringAsync(new Uri("https://nekos.life/api/v2/fact")).ConfigureAwait(false);
            dynamic item = JObject.Parse(rawresp);
            var fact = item["fact"];
            if (fact is null) return null;
            return fact;
        }
    }
}