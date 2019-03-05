using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Kitsu.Anime;
using Kitsu.Manga;
using Skuld.APIS.Extensions;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Utilities;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using SysEx.Net;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TraceMoe.NET;

namespace Skuld.Bot.Commands
{
    [Group]
    public class Weeb : InteractiveBase<SkuldCommandContext>
    {
        public Locale Locale { get; set; }
        public SysExClient SysExClient { get; set; }

        [Command("anime"), Summary("Gets information about an anime")]
        public async Task GetAnime([Remainder]string animetitle)
        {
            var usr = Context.DBUser;
            var loc = Locale.GetLocale(usr.Language ?? Locale.defaultLocale);

            var raw = await Anime.GetAnimeAsync(animetitle);
            var data = raw.Data;
            if (data.Count > 1) // do pagination
            {
                var pages = data.PaginateList();

                IUserMessage sentmessage = null;

                if (pages.Count() > 1)
                {
                    sentmessage = await PagedReplyAsync(new PaginatedMessage
                    {
                        Title = loc.GetString("SKULD_SEARCH_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Pages = pages
                    }, true);
                }
                else
                {
                    sentmessage = await ReplyAsync(null, false, new EmbedBuilder
                    {
                        Title = loc.GetString("SKULD_SEARCH_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Description = pages[0]
                    }.Build());
                }

                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                if (response == null)
                {
                    await sentmessage.DeleteAsync();
                }

                var selection = Convert.ToInt32(response.Content);

                var anime = data[selection - 1];

                await anime.ToEmbed(loc).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                var anime = data[0];

                await anime.ToEmbed(loc).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("manga"), Summary("Gets information about a manga")]
        public async Task GetMangaAsync([Remainder]string mangatitle)
        {
            var usr = Context.DBUser;
            var loc = Locale.GetLocale(usr.Language ?? Locale.defaultLocale);

            var raw = await Manga.GetMangaAsync(mangatitle);
            var data = raw.Data;
            if (data.Count > 1) // do pagination
            {
                var pages = data.PaginateList();

                IUserMessage sentmessage = null;

                if (pages.Count > 1)
                {
                    sentmessage = await PagedReplyAsync(new PaginatedMessage
                    {
                        Title = loc.GetString("SKULD_SEARCH_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Pages = pages
                    }, true);
                }
                else
                {
                    sentmessage = await ReplyAsync(null, false, new EmbedBuilder
                    {
                        Title = loc.GetString("SKULD_SEARCH_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Description = pages[0]
                    }.Build());
                }

                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                if (response == null)
                {
                    await sentmessage.DeleteAsync();
                }

                var selection = Convert.ToInt32(response.Content);

                var manga = data[selection - 1];

                await manga.ToEmbed(loc).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                var manga = data[0];

                await manga.ToEmbed(loc).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("weebgif"), Summary("Gets a weeb gif")]
        public async Task WeebGif()
        {
            var gif = await SysExClient.GetWeebReactionGifAsync();

            await new EmbedBuilder
            {
                ImageUrl = gif,
                Color = EmbedUtils.RandomColor()
            }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("whatanime"), Summary("Searches Trace.Moe")]
        public async Task WhatAnime(Uri url = null)
        {
            if(url == null)
            {
                if (Context.Message.Attachments.Count() > 0)
                {
                    var attach = Context.Message.Attachments.First();

                    var data = await new TraceMoe.NET.ApiConversion().TraceAnimeByUrlAsync(attach.Url);

                    if (data.docs.Count() > 0)
                    {
                        var ordered = data.docs.OrderByDescending(x => x.similarity);
                        var mostLikely = ordered.First();

                        var message = $"The image is most likely from: {mostLikely.title}/{mostLikely.title_romaji}\n" +
                            $"Similarity Rating: {string.Format("{0:0.00}", Math.Round(mostLikely.similarity * 100))}%\n" +
                            "Other potential candidates:\n";

                        var take = ordered.Skip(1).Where(x => x.similarity < mostLikely.similarity).Take(5);

                        foreach (var took in take)
                        {
                            message += $"{took.title}/{took.title_romaji} - {string.Format("{0:0.00}", Math.Round(took.similarity * 100))}%\n";
                        }

                        await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        await "Couldn't find anything with the image provided".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
            }
            else
            {
                var data = await new ApiConversion().TraceAnimeByUrlAsync(url.OriginalString);

                if (data.docs.Count() > 0)
                {
                    var ordered = data.docs.OrderByDescending(x => x.similarity);
                    var mostLikely = ordered.First();

                    var message = $"The image is most likely from: {mostLikely.title}/{mostLikely.title_romaji}\n" +
                        $"Similarity Rating: {string.Format("{0:0.00}", Math.Round(mostLikely.similarity * 100))}%\n" +
                        "Other potential candidates:\n";

                    var take = ordered.Skip(1).Where(x=>x.similarity < mostLikely.similarity).Take(5);

                    foreach (var took in take)
                    {
                        message += $"{took.title}/{took.title_romaji} - {string.Format("{0:0.00}", Math.Round(took.similarity * 100))}%\n";
                    }

                    await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    await "Couldn't find anything with the image provided".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
            }
        }
    }
}