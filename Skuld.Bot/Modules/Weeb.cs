using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Kitsu.Anime;
using Kitsu.Manga;
using Miki.API.Images;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Extensions;
using Skuld.Core.Extensions;
using Skuld.Models;
using Skuld.Services.Globalization;
using Skuld.Services.Messaging.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TraceMoe.NET;
using TraceMoe.NET.DataStructures;

namespace Skuld.Bot.Modules
{
	[Group, Name("Weeb")]
	[Remarks("📺 Weeb paraphernalia")]
    public class WeebModule : InteractiveBase<ShardedCommandContext>
    {
        public Locale Locale { get; set; }
		public ImghoardClient Imghoard { get; set; }

        [
            Command("anime"),
            Summary("Gets information about an anime"),
            Ratelimit(20, 1, Measure.Minutes),
            Usage("JoJo's Bizarre Adventure")
        ]
        public async Task GetAnime([Remainder]string animetitle)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database
                .InsertOrGetUserAsync(Context.User)
            .ConfigureAwait(false);

            var loc = Locale.GetLocale(
                usr.Language ?? Locale.DefaultLocale
            );

            var raw = await Anime
                .GetAnimeAsync(animetitle)
            .ConfigureAwait(false);

            var data = raw.Data;
            if (data.Count > 1) // do pagination
            {
                var pages = data.PaginateList(25);

                IUserMessage sentmessage = await ReplyAsync(null, false,
                    EmbedExtensions
                        .FromMessage(
                            loc.GetString("SKULD_SEARCH_MKSLCTN") + " 30s",
                            pages[0],
                            Context
                        )
                        .WithColor(Color.Purple)
                    .Build()
                ).ConfigureAwait(false);

                var response = await NextMessageAsync(
                    true,
                    true,
                    TimeSpan.FromSeconds(30)
                ).ConfigureAwait(false);

                if (response is null)
                {
                    await sentmessage.DeleteAsync().ConfigureAwait(false);
                    return;
                }

                var selection = Convert.ToInt32(response.Content);

                var anime = data[selection - 1];

                await anime
                    .ToEmbed(loc)
                    .QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }
            else
            {
                var anime = data[0];

                await anime
                    .ToEmbed(loc)
                    .QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }
        }

        [
            Command("manga"), 
            Summary("Gets information about a manga"),
            Ratelimit(20, 1, Measure.Minutes),
            Usage("JoJo's Bizarre Adventure")
        ]
        public async Task GetMangaAsync([Remainder]string mangatitle)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database
                .InsertOrGetUserAsync(Context.User)
            .ConfigureAwait(false);

            var loc = Locale.GetLocale(
                usr.Language ?? Locale.DefaultLocale
            );

            var raw = await Manga
                .GetMangaAsync(mangatitle)
            .ConfigureAwait(false);
            var data = raw.Data;
            if (data.Count > 1) // do pagination
            {
                var pages = data.PaginateList(25);

                IUserMessage sentmessage = await ReplyAsync(
                    null,
                    false,
                    EmbedExtensions
                        .FromMessage(
                            loc.GetString("SKULD_SEARCH_MKSLCTN") + " 30s",
                            pages[0],
                            Context
                        )
                        .WithColor(Color.Purple)
                    .Build()
                ).ConfigureAwait(false);

                var response = await NextMessageAsync(
                    true,
                    true,
                    TimeSpan.FromSeconds(30)
                ).ConfigureAwait(false);

                if (response is null)
                {
                    await sentmessage.DeleteAsync().ConfigureAwait(false);
                    return;
                }

                var selection = Convert.ToInt32(response.Content);

                var manga = data[selection - 1];

                await manga
                    .ToEmbed(loc)
                    .QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }
            else
            {
                var manga = data[0];

                await manga
                        .ToEmbed(loc)
                        .QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }
        }

        [
            Command("weebgif"),
            Summary("Gets a weeb gif"),
            Ratelimit(20, 1, Measure.Minutes)
        ]
        public async Task WeebGif()
		{
			var gif = await Imghoard.GetImagesAsync();

			await
				EmbedExtensions
				.FromImage(
					gif.Images.Random().Url,
					EmbedExtensions.RandomEmbedColor(),
					Context
				)
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

        [
            Command("whatanime"),
            Summary("Searches Trace.Moe"),
            Ratelimit(20, 1, Measure.Minutes),
            Usage("[Image/Link] Screenshot of Anime")
        ]
        public async Task WhatAnime(Uri url = null)
        {
            if (url is null)
            {
                if (Context.Message.Attachments.Any())
                {
                    var attach = Context.Message.Attachments.First();

                    var data = await 
                        new ApiConversion()
                        .TraceAnimeByUrlAsync(attach.Url)
                    .ConfigureAwait(false);

                    if (data.docs.Any())
                    {
                        await GetWhatAnimeMessage(
                                data.docs.OrderByDescending(x => x.similarity)
                            )
                            .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                    }
                    else
                    {
                        await 
                            "Couldn't find anything with the image provided"
                            .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var data = await 
                    new ApiConversion()
                    .TraceAnimeByUrlAsync(url.OriginalString)
                .ConfigureAwait(false);

                if (data.docs.Any())
                {
                    await GetWhatAnimeMessage(
                        data.docs.OrderByDescending(x => x.similarity)
                        )
                        .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
                }
                else
                {
                    await 
                        "Couldn't find anything with the image provided"
                        .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
                }
            }
        }

        private string GetWhatAnimeMessage(IOrderedEnumerable<Doc> results)
        {
            var mostLikely = results.First();

            var mlpercentage = Math.Round(mostLikely.similarity * 100);
            var mlstringedPerc = string.Format("{0:0.00}", mlpercentage);

            var message = new StringBuilder(
                $"The image is most likely from: " +
                $"{mostLikely.title}/{mostLikely.title_romaji}\n" +
                $"Similarity Rating: {mlstringedPerc}%"
            );

            var take = results
                .Skip(1)
                .Where(x => x.similarity < mostLikely.similarity)
            .Take(5);

            if (!take.All(x => x.mal_id == mostLikely.mal_id))
            {
                var appendix = new StringBuilder(
                    "\nOther potential candidates:\n"
                );

                foreach (var took in take)
                {
                    var percentage = Math.Round(took.similarity * 100);
                    var stringedPerc = string.Format("{0:0.00}", percentage);

                    appendix = appendix.Append(
                        $"{took.title}/{took.title_romaji} - {stringedPerc}%\n"
                    );
                }
                message = message.Append(appendix);
            }

            return message.ToString();
        }
    }
}