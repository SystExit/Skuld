﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using Skuld.APIS;
using Skuld.Tools;
using System.Collections.Generic;
using System.Web;
using Discord.Addons.Interactive;
using Skuld.Models.API.MAL;

namespace Skuld.Modules
{
    [Group, Name("Search")]
    public class WeebModule : InteractiveBase
    {
        private static int timeout = 20;
        private MangaArr MangaArray;
        [Command("manga", RunMode = RunMode.Async), Summary("Gets a manga from MyAnimeList.Net")]
        public async Task MangaGet([Remainder]string mangatitle)
        {
            var skuser = await Bot.Database.GetUserAsync(Context.User.Id);
            var pages = new List<string>();
            MangaArray = await MALAPI.GetMangasAsync(mangatitle);
            int manganode = MangaArray.Entry.Count;
            if (manganode <= 0)
            {
                await ReplyAsync(Locale.GetLocale(skuser.Language).GetString("SKULD_SEARCH_NO_RESULTS").Replace("{{result}}", mangatitle));
            }
            if (manganode > 1)
            {
                IMessage msg = null;
                var msgs = new List<IMessage>();
                string entrymessage = Locale.GetLocale(skuser.Language).GetString("SKULD_SEARCH_WEEB_MKSLCTN") + " " + timeout + "s\n";
                int count = 0;
                var entries = new List<string>();
                foreach (var item in MangaArray.Entry)
                {
                    count++;
                    entries.Add($"**{count}. {item.Title}**");
                }
                if (entries.Count >= 10)
                {
                    string tempstring = "";
                    var clonedarr = entries;
                    for (int x = 0; x < entries.Count; x++)
                    {
                        if (entries.LastOrDefault() == entries[x])
                        { tempstring += clonedarr[x]; }
                        else
                        { tempstring += clonedarr[x] + "\n\n"; }
                        if (x > 1)
                        {
                            if ((x - 1) % 10 == 0)
                            {
                                pages.Add(tempstring);
                                tempstring = "";
                            }
                            else if (x == entries.Count())
                            {
                                pages.Add(tempstring);
                                tempstring = "";
                            }
                        }
                    }

                    var pagedMessage = new PaginatedMessage()
                    {
                        Title = entrymessage,
                        Color = Tools.Tools.RandomColor(),
                        Pages = pages
                    };
                    msg = await PagedReplyAsync(pager: pagedMessage, fromSourceUser: true);
                }
                else
                {
                    entrymessage += String.Join(Environment.NewLine, entries.ToArray());
                    msg = await MessageHandler.SendChannelAsync(Context.Channel, entrymessage);
                }
                var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout:TimeSpan.FromSeconds(timeout));
                await GetMangaAtPosition(Convert.ToInt32(response.Content), response.Channel).ConfigureAwait(false);

                if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                    await response.DeleteAsync();
                }
                if (msgs.Count > 0)
                {
                    foreach (var m in msgs)
                    { await m.DeleteAsync(); }
                }
                if (msg != null)
                { await msg.DeleteAsync(); }
            }
            else
            {
                await GetMangaAtZeroNonSearch();
            }
        }

        private async Task GetMangaAtZeroNonSearch() =>
            await SendManga(MangaArray.Entry.First());
        private async Task GetMangaAtPosition(int position, IMessageChannel channel) =>
            await SendManga(MangaArray.Entry.ElementAt(position - 1), channel);

        public async Task SendManga(Manga mango, IMessageChannel channel = null)
        {
            try
            {
                var user = await Bot.Database.GetUserAsync(Context.User.Id);
                var locale = Locale.GetLocale(user.Language ?? Locale.defaultLocale);
                var embed = new EmbedBuilder
                {
                    Color = Tools.Tools.RandomColor(),
                    Author = new EmbedAuthorBuilder
                    {
                        Url = $"https://myanimelist.net/manga/{mango.Id}/",
                        Name = mango.Title
                    },
                    ThumbnailUrl = mango.Image
                };
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_ENGTITLE"), Tools.Tools.CheckForEmptyWithLocale(mango.EnglishTitle, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SYNON"), Tools.Tools.CheckForEmptyWithLocale(mango.Synonyms, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_CHPS"), Tools.Tools.CheckForEmptyWithLocale(mango.Chapters, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_VOLS"), Tools.Tools.CheckForEmptyWithLocale(mango.Volumes, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SDATE"), Tools.Tools.CheckForEmptyWithLocale(mango.StartDate, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_EDATE"), Tools.Tools.CheckForEmptyWithLocale(mango.EndDate, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SCORE"), Tools.Tools.CheckForEmptyWithLocale(mango.Score, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SYNOP"), Tools.Tools.CheckForEmptyWithLocale(HttpUtility.HtmlDecode(mango.Synopsis.Split('<')[0]), locale), true);

                await MessageHandler.SendChannelAsync(channel ?? Context.Channel, "", embed.Build());
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("Weeb-Mng", "Something happened", LogSeverity.Error, ex));
            }
        }

        private AnimeArr AnimeArray;
        [Command("anime", RunMode = RunMode.Async), Summary("Gets an anime from MyAnimeList.Net")]
        public async Task Animuget([Remainder]string animetitle)
        {
            var skuser = await Bot.Database.GetUserAsync(Context.User.Id);
            var pages = new List<string>();
            AnimeArray = await MALAPI.GetAnimesAsync(animetitle);
            int animenodes = AnimeArray.Entry.Count;
            if (animenodes == 0 || AnimeArray == null)
                await ReplyAsync(Locale.GetLocale(skuser.Language).GetString("SKULD_SEARCH_NO_RESULTS").Replace("{{result}}", animetitle));
            if (animenodes > 1)
            {
                IMessage msg = null;
                string entrymessage = Locale.GetLocale(skuser.Language).GetString("SKULD_SEARCH_WEEB_MKSLCTN") + " " + timeout + "s\n";
                int count = 0;
                var entries = new List<string>();
                foreach (var item in AnimeArray.Entry)
                {
                    count++;
                    entries.Add($"**{count}. {item.Title}**");
                }
                if (entries.Count >= 10)
                {
                    string tempstring = "";
                    var clonedarr = entries;
                    for (int x = 1; x <= entries.Count; x++)
                    {
                        if (entries.LastOrDefault() == entries[x - 1])
                        { tempstring += clonedarr[x - 1]; }
                        else
                        { tempstring += clonedarr[x - 1] + "\n\n"; }
                        if ((x - 1) % 10 == 0)
                        { pages.Add(tempstring);
                            tempstring = ""; }
                        else if (x == entries.Count())
                        { pages.Add(tempstring);
                            tempstring = ""; }
                    }

                    var pagedMessage = new PaginatedMessage()
                    {
                        Title = entrymessage,
                        Color = Tools.Tools.RandomColor(),
                        Pages = pages
                    };
                    msg = await PagedReplyAsync(pager: pagedMessage, fromSourceUser: true);
                }
                else
                {
                    entrymessage += String.Join(Environment.NewLine, entries.ToArray());
                    msg = await MessageHandler.SendChannelAsync(Context.Channel, entrymessage);
                }
                var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout: TimeSpan.FromSeconds(timeout));
                await GetAnimeAtPosition(Convert.ToInt32(response.Content), response.Channel).ConfigureAwait(false);

                if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                    await response.DeleteAsync();
                }
                if (msg != null)
                    await msg.DeleteAsync();
            }
            else
            {
                await GetAnimeAtZeroNonSearch().ConfigureAwait(false);
            }
        }

        public async Task GetAnimeAtZeroNonSearch() =>
            await SendAnime(AnimeArray.Entry.FirstOrDefault()).ConfigureAwait(false);
        private async Task GetAnimeAtPosition(int position, IMessageChannel channel) =>
            await SendAnime(AnimeArray.Entry[position - 1], channel).ConfigureAwait(false);

        private async Task SendAnime(Anime animu, IMessageChannel channel = null)
        {
            try
            {
                var user = await Bot.Database.GetUserAsync(Context.User.Id);
                var locale = Locale.GetLocale(user.Language ?? Locale.defaultLocale);

                var embed = new EmbedBuilder
                {
                    Color = Tools.Tools.RandomColor(),
                    Author = new EmbedAuthorBuilder
                    {
                        Url = $"https://myanimelist.net/anime/{animu.Id}/",
                        Name = animu.Title
                    },
                    ThumbnailUrl = animu.Image
                };

                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_ENGTITLE"), Tools.Tools.CheckForEmptyWithLocale(animu.EnglishTitle,locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SYNON"), Tools.Tools.CheckForEmptyWithLocale(animu.Synonyms, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_EPS"), Tools.Tools.CheckForEmptyWithLocale(animu.Episodes, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SDATE"), Tools.Tools.CheckForEmptyWithLocale(animu.StartDate, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_EDATE"), Tools.Tools.CheckForEmptyWithLocale(animu.EndDate, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SCORE"), Tools.Tools.CheckForEmptyWithLocale(animu.Score, locale), true);
                embed.AddField(locale.GetString("SKULD_SEARCH_WEEB_SYNOP"), Tools.Tools.CheckForEmptyWithLocale(HttpUtility.HtmlDecode(animu.Synopsis.Split('<')[0]), locale), true);

                await MessageHandler.SendChannelAsync(channel ?? Context.Channel, "", embed.Build());
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("Weeb-Anm", "Something happened", LogSeverity.Error, ex));
            }
        }
        
        [Command("weebgif", RunMode = RunMode.Async), Summary("Gets a weeb gif")]
        public async Task WeebGif()
        {
            await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder
            {
                ImageUrl = await APIWebReq.ReturnString(new Uri("http://gaia.systemexit.co.uk/gifs/reactions/")),
                Color = Tools.Tools.RandomColor()
            }.Build());
        }
    }
}
