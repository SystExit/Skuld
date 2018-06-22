using System.Threading.Tasks;
using Discord.Commands;
using Skuld.APIS;
using Skuld.Tools;
using Discord;
using System.Text.RegularExpressions;
using Discord.Addons.Interactive;
using Skuld.Services;
using Skuld.Extensions;
using SysEx.Net;
using Booru.Net;

namespace Skuld.Modules
{
    [Group, RequireNsfw]
    public class Lewd : InteractiveBase<ShardedCommandContext>
    {
        public AnimalAPIS Animals { get; set; }
        public SysExClient SysExClient { get; set; }
        public BooruClient BooruClient { get; set; }

        [Command("lewdneko"), Summary("Lewd Neko Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdNeko()
        {
            var neko = await Animals.GetLewdNekoAsync();
            if (neko != null)
                await Context.Channel.ReplyAsync(new EmbedBuilder { ImageUrl = neko }.Build());
            else
                await Context.Channel.ReplyAsync("Hmmm <:Thunk:350673785923567616>, I got an empty response.");
        }

        [Command("lewdkitsune"), Summary("Lewd Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdKitsune()
        {
            var kitsu = await SysExClient.GetLewdKitsuneAsync();
            StatsdClient.DogStatsd.Increment("web.get");
            await Context.Channel.ReplyAsync("", new EmbedBuilder { ImageUrl = kitsu }.Build());
        }

        [Command("danbooru"), Summary("Gets stuff from danbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dan")]
        public async Task Danbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
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
                        await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                        return;
                    }
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }

        [Command("gelbooru"), Summary("Gets stuff from gelbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("gel")]
        public async Task Gelbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
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
                        await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                        return;
                    }
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }

        [Command("rule34"), Summary("Gets stuff from rule34"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("r34")]
        public async Task R34(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
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
                        await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                        return;
                    }
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }

        [Command("e621"), Summary("Gets stuff from e621"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task E621(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
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
                        await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                        return;
                    }
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }

        [Command("konachan"), Summary("Gets stuff from konachan"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("kona", "kc")]
        public async Task KonaChan(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
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
                        await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                        return;
                    }
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }

        [Command("yandere"), Summary("Gets stuff from yandere"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Yandere(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
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

                        await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                        return;
                    }
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }

        [Command("real"), Summary("Gets stuff from Realbooru"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Realbooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
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
                        await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                        return;
                    }
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }
    }
}
