using Booru.Net;
using Discord;
using Discord.Commands;
using Skuld.APIS;
using Skuld.APIS.Extensions;
using Skuld.APIS.NekoLife.Models;
using Skuld.Core.Extensions;
using Skuld.Services.Discord.Attributes;
using Skuld.Services.Discord.Preconditions;
using Skuld.Services.Messaging.Extensions;
using SysEx.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, Name("Lewd"), RequireNsfw, RequireEnabledModule]
    public class LewdModule : ModuleBase<ShardedCommandContext>
    {
        public SysExClient SysExClient { get; set; }
        public DanbooruClient DanbooruClient { get; set; }
        public GelbooruClient GelbooruClient { get; set; }
        public Rule34Client Rule34Client { get; set; }
        public E621Client E621Client { get; set; }
        public KonaChanClient KonaChanClient { get; set; }
        public YandereClient YandereClient { get; set; }
        public RealbooruClient RealbooruClient { get; set; }
        public NekosLifeClient NekosLifeClient { get; set; }

        [Command("lewdneko"), Summary("Lewd Neko Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdNeko()
        {
            var neko = await NekosLifeClient.GetAsync(NekoImageType.LewdNeko).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            if (neko != null)
                await EmbedExtensions.FromImage(neko, Color.Purple, Context).QueueMessageAsync(Context).ConfigureAwait(false);
            else
                await EmbedExtensions.FromError("Hmmm <:Thunk:350673785923567616> I got an empty response. Try again.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("lewdkitsune"), Summary("Lewd Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task LewdKitsune()
        {
            var kitsu = await SysExClient.GetLewdKitsuneAsync().ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            await EmbedExtensions.FromMessage(Context).WithImageUrl(kitsu).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #region ImageBoards

        [Command("danbooru"), Summary("Gets stuff from danbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dan")]
        [Usage("wholesome")]
        public async Task Danbooru(params string[] tags)
        {
            var test = tags.ContainsBlacklistedTags();
            if (test.Successful)
            {
                if (test.Data.Count() > 1)
                    await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var posts = await DanbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();

            await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("gelbooru"), Summary("Gets stuff from gelbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("gel")]
        [Usage("wholesome")]
        public async Task Gelbooru(params string[] tags)
        {
            var test = tags.ContainsBlacklistedTags();
            if (test.Successful)
            {
                if (test.Data.Count() > 1)
                    await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var posts = await GelbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();

            await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("rule34"), Summary("Gets stuff from rule34"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("r34")]
        [Usage("wholesome")]
        public async Task R34(params string[] tags)
        {
            var test = tags.ContainsBlacklistedTags();
            if (test.Successful)
            {
                if (test.Data.Count() > 1)
                    await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var posts = await Rule34Client.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();

            await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("e621"), Summary("Gets stuff from e621"), Ratelimit(20, 1, Measure.Minutes)]
        [Usage("wholesome")]
        public async Task E621(params string[] tags)
        {
            var test = tags.ContainsBlacklistedTags();
            if (test.Successful)
            {
                if (test.Data.Count() > 1)
                    await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var posts = await E621Client.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var post = posts.Where(x => x.Tags.All(z=>!z.Value.ContainsBlacklistedTags().Successful)).RandomValue();

            await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("konachan"), Summary("Gets stuff from konachan"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("kona", "kc")]
        [Usage("wholesome")]
        public async Task KonaChan(params string[] tags)
        {
            if(tags.Count() > 6)
            {
                await $"Cannot process more than 6 tags at a time".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
            var test = tags.ContainsBlacklistedTags();
            if (test.Successful)
            {
                if (test.Data.Count() > 1)
                    await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var posts = await KonaChanClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();

            await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("yandere"), Summary("Gets stuff from yandere"), Ratelimit(20, 1, Measure.Minutes)]
        [Usage("wholesome")]
        public async Task Yandere(params string[] tags)
        {
            var test = tags.ContainsBlacklistedTags();
            if (test.Successful)
            {
                if (test.Data.Count() > 1)
                    await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var posts = await YandereClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();

            await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("real"), Summary("Gets stuff from Realbooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Usage("wholesome")]
        public async Task Realbooru(params string[] tags)
        {
            var test = tags.ContainsBlacklistedTags();
            if (test.Successful)
            {
                if (test.Data.Count() > 1)
                    await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var posts = await RealbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");

            if (posts == null)
            {
                await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();

            await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("hentaibomb"), Summary("\"bombs\" the chat with images from all boorus"), Ratelimit(20, 1, Measure.Minutes), Priority(1)]
        [Usage("wholesome")]
        public async Task HentaiBomb([Remainder]string tags = null)
        {
            List<string> localTags = new List<string>();
            if (tags != null)
            {
                var splittags = tags.Split(' ');
                localTags = splittags.ToList();
                var test = localTags.ContainsBlacklistedTags();
                if (test.Successful)
                {
                    if (test.Data.Count() > 1)
                        await $"Your tags contains a banned tag, please remove it.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                    else
                        await $"Your tags contains {test.Data.Count()} banned tags, please remove them.\nBanned Tags Found: {string.Join(", ", test.Data)}".QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }
            }
            string msg = "";

            var posts = await YandereClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts != null)
            {
                var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();
                if (post != null)
                {
                    msg += $"Yande.re: {post.GetMessage(Context, true)}\n";
                }
            }

            var posts2 = await KonaChanClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts2 != null)
            {
                var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();
                if (post != null)
                {
                    msg += $"Konachan {post.GetMessage(Context, true)}\n";
                }
            }

            var posts3 = await E621Client.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts3 != null)
            {
                var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();
                if (post != null)
                {
                    msg += $"E621: {post.GetMessage(Context, true)}\n";
                }
            }

            var posts4 = await Rule34Client.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts4 != null)
            {
                var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();
                if (post != null)
                {
                    msg += $"R34: {post.GetMessage(Context, true)}\n";
                }
            }

            var posts5 = await GelbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts5 != null)
            {
                var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();
                if (post != null)
                {
                    msg += $"Gelbooru: {post.GetMessage(Context, true)}\n";
                }
            }

            var posts6 = await DanbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
            StatsdClient.DogStatsd.Increment("web.get");
            if (posts6 != null)
            {
                var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).RandomValue();
                if (post != null)
                {
                    msg += $"Danbooru: {post.GetMessage(Context, true)}\n";
                }
            }

            await msg.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion ImageBoards
    }
}