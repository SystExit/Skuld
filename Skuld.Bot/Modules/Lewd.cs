using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.APIS;
using Skuld.APIS.Extensions;
using Skuld.APIS.NekoLife.Models;
using Skuld.Discord.Attributes;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using SysEx.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireNsfw, RequireEnabledModule]
    public class Lewd : InteractiveBase<SkuldCommandContext>
    {
        public SysExClient SysExClient { get; set; }
        public BooruClient BooruClient { get; set; }
        public NekosLifeClient NekosLifeClient { get; set; }

        [Command("lewdneko"), Summary("Lewd Neko Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdNeko()
        {
            var neko = await NekosLifeClient.GetAsync(NekoImageType.LewdNeko);
            if (neko != null)
                await new EmbedBuilder { ImageUrl = neko }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else
                await "Hmmm <:Thunk:350673785923567616>, I got an empty response.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
        }

        [Command("lewdkitsune"), Summary("Lewd Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdKitsune()
        {
            var kitsu = await SysExClient.GetLewdKitsuneAsync();
            StatsdClient.DogStatsd.Increment("web.get");
            await new EmbedBuilder { ImageUrl = kitsu }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("danbooru"), Summary("Gets stuff from danbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dan")]
        public async Task Danbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
            var cleantags = tags.AddBlacklistedTags();

            var posts = await BooruClient.GetDanbooruImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await "Couldn't find an image.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            var post = posts.GetRandomItem();

            await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("gelbooru"), Summary("Gets stuff from gelbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("gel")]
        public async Task Gelbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
            var cleantags = tags.AddBlacklistedTags();

            var posts = await BooruClient.GetGelbooruImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await "Couldn't find an image.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            var post = posts.GetRandomItem();

            await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("rule34"), Summary("Gets stuff from rule34"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("r34")]
        public async Task R34(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
            var cleantags = tags.AddBlacklistedTags();

            var posts = await BooruClient.GetRule34ImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await "Couldn't find an image.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            var post = posts.GetRandomItem();

            await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("e621"), Summary("Gets stuff from e621"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task E621(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
            var cleantags = tags.AddBlacklistedTags();

            var posts = await BooruClient.GetE621ImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await "Couldn't find an image.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            var post = posts.GetRandomItem();

            await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("konachan"), Summary("Gets stuff from konachan"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("kona", "kc")]
        public async Task KonaChan(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
            var cleantags = tags.AddBlacklistedTags();

            var posts = await BooruClient.GetKonaChanImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await "Couldn't find an image.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            var post = posts.GetRandomItem();

            await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("yandere"), Summary("Gets stuff from yandere"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Yandere(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
            var cleantags = tags.AddBlacklistedTags();

            var posts = await BooruClient.GetYandereImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await "Couldn't find an image.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            var post = posts.GetRandomItem();

            await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("real"), Summary("Gets stuff from Realbooru"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Realbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }
            var cleantags = tags.AddBlacklistedTags();

            var posts = await BooruClient.GetRealBooruImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await "Couldn't find an image.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                return;
            }

            var post = posts.GetRandomItem();

            await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("hentaibomb"), Summary("\"bombs\" the chat with images from all boorus"), Ratelimit(20, 1, Measure.Minutes), Priority(2)]
        public async Task HentaiBomb()
            => await HentaiBomb(null);

        [Command("hentaibomb"), Summary("\"bombs\" the chat with images from all boorus"), Ratelimit(20, 1, Measure.Minutes), Priority(1)]
        public async Task HentaiBomb(params string[] tags)
        {
            List<string> localTags;
            if (tags != null)
            {
                localTags = tags.ToList();
                if (tags.ContainsBlacklistedTags())
                {
                    await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    return;
                }
            }
            else
            {
                localTags = new List<string>();
            }
            string msg = "";

            var cleantags = localTags.AddBlacklistedTags();

            var posts = await BooruClient.GetYandereImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts != null)
            {
                var post = posts.GetRandomItem();
                if (post != null)
                {
                    msg += post.GetMessage(post.PostUrl) + "\n";
                }
            }

            var posts2 = await BooruClient.GetKonaChanImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts2 != null)
            {
                var post = posts2.GetRandomItem();
                if (post != null)
                {
                    msg += post.GetMessage(post.PostUrl) + "\n";
                }
            }

            var posts3 = await BooruClient.GetE621ImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts3 != null)
            {
                var post = posts3.GetRandomItem();
                if (post != null)
                {
                    msg += post.GetMessage(post.PostUrl) + "\n";
                }
            }

            var posts4 = await BooruClient.GetRule34ImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts4 != null)
            {
                var post = posts4.GetRandomItem();
                if (post != null)
                {
                    msg += post.GetMessage(post.PostUrl) + "\n";
                }
            }

            var posts5 = await BooruClient.GetGelbooruImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts5 != null)
            {
                var post = posts5.GetRandomItem();
                if (post != null)
                {
                    msg += post.GetMessage(post.PostUrl) + "\n";
                }
            }

            var posts6 = await BooruClient.GetDanbooruImagesAsync(cleantags);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts6 != null)
            {
                var post = posts6.GetRandomItem();
                if (post != null)
                {
                    msg += post.GetMessage(post.PostUrl) + "\n";
                }
            }

            await msg.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }
    }
}