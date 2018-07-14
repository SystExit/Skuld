using Discord;
using Discord.Commands;
using Skuld.Commands;
using System.Threading.Tasks;
using Skuld.Extensions;
using Skuld.APIS;
using Skuld.Core.Commands.Attributes;
using SysEx.Net;
using Booru.Net;
using Skuld.APIS.NekoLife.Models;

namespace Skuld.Modules
{
    [Group, RequireNsfw]
    public class Lewd : SkuldBase<ShardedCommandContext>
    {
        public SysExClient SysExClient { get; set; }
        public BooruClient BooruClient { get; set; }
        public NekosLifeClient NekosLifeClient { get; set; }

        [Command("lewdneko"), Summary("Lewd Neko Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdNeko()
        {
            var neko = await NekosLifeClient.GetAsync(NekoImageType.LewdNeko);
            if (neko != null)
                await ReplyAsync(Context.Channel, new EmbedBuilder { ImageUrl = neko }.Build());
            else
                await ReplyAsync(Context.Channel, "Hmmm <:Thunk:350673785923567616>, I got an empty response.");
        }

        [Command("lewdkitsune"), Summary("Lewd Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdKitsune()
        {
            var kitsu = await SysExClient.GetLewdKitsuneAsync();
            StatsdClient.DogStatsd.Increment("web.get");
            await ReplyAsync(Context.Channel, new EmbedBuilder { ImageUrl = kitsu }.Build());
        }

        [Command("danbooru"), Summary("Gets stuff from danbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dan")]
        public async Task Danbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetDanbooruImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                if (posts != null)
                {
                    var post = posts.GetRandomImage();
                    if (post != null)
                    {
                        await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                        return;
                    }
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
            }
        }

        [Command("gelbooru"), Summary("Gets stuff from gelbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("gel")]
        public async Task Gelbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetGelbooruImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                if (posts != null)
                {
                    var post = posts.GetRandomImage();
                    if (post != null)
                    {
                        await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                        return;
                    }
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
            }
        }

        [Command("rule34"), Summary("Gets stuff from rule34"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("r34")]
        public async Task R34(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetRule34ImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                if (posts != null)
                {
                    var post = posts.GetRandomImage();
                    if (post != null)
                    {
                        await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                        return;
                    }
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
            }
        }

        [Command("e621"), Summary("Gets stuff from e621"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task E621(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetE621ImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                if (posts != null)
                {
                    var post = posts.GetRandomImage();
                    if (post != null)
                    {
                        await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                        return;
                    }
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
            }
        }

        [Command("konachan"), Summary("Gets stuff from konachan"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("kona", "kc")]
        public async Task KonaChan(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetKonaChanImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                if (posts != null)
                {
                    var post = posts.GetRandomImage();
                    if (post != null)
                    {
                        await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                        return;
                    }
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
            }
        }

        [Command("yandere"), Summary("Gets stuff from yandere"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Yandere(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetYandereImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                if (posts != null)
                {
                    var post = posts.GetRandomImage();
                    if (post != null)
                    {
                        await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                        return;
                    }
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
            }
        }

        [Command("real"), Summary("Gets stuff from Realbooru"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Realbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetRealBooruImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                if (posts != null)
                {
                    var post = posts.GetRandomImage();
                    if (post != null)
                    {
                        await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                        return;
                    }
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
            }
        }
    }
}
