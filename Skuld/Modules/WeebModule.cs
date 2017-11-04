using System;
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
            var skuser = await SqlTools.GetUserAsync(Context.User.Id);
            var pages = new List<string>();
            MangaArray = await MALAPI.GetMangasAsync(mangatitle);
            int manganode = MangaArray.Entry.Count;
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
                            tempstring += clonedarr[x];
                        else
                            tempstring += clonedarr[x] + "\n\n";
                        if (x > 1)
                        {
                            if (x - 1 % 10 == 0)
                            {
                                pages.Add(tempstring); tempstring = "";
                            }
                            else if (x == entries.Count())
                            {
                                pages.Add(tempstring); tempstring = "";
                            }
                        }
                        Console.WriteLine(tempstring);
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
                    msg = await MessageHandler.SendChannel(Context.Channel, entrymessage);
                }
                var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout:TimeSpan.FromSeconds(timeout));
                await GetMangaAtPosition(Convert.ToInt32(response.Content), response.Channel);

                if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                    await response.DeleteAsync();
                }
                if (msgs.Count > 0)
                    foreach (var m in msgs)
                        await m.DeleteAsync();
                if (msg != null)
                    await msg.DeleteAsync();
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
        public async Task SendManga(Manga mango, IMessageChannel channel = null) =>
			await MessageHandler.SendChannel(channel ?? Context.Channel, "", BuildMALEmbed(mango, Locale.GetLocale((await SqlTools.GetUserAsync(Context.User.Id)).Language)));

        private AnimeArr AnimeArray;
        [Command("anime", RunMode = RunMode.Async), Summary("Gets an anime from MyAnimeList.Net")]
        public async Task Animuget([Remainder]string animetitle)
        {
			var skuser = await SqlTools.GetUserAsync(Context.User.Id);
			var pages = new List<string>();
			AnimeArray = await MALAPI.GetAnimesAsync(animetitle);
            int animenodes = AnimeArray.Entry.Count;
            try
            {
                if (animenodes == 0)
                    await ReplyAsync(Locale.GetLocale(skuser.Language).GetString("SKULD_SEARCH_WEEB_EMPTY"));
                if (animenodes > 1)
                {
                    IMessage msg = null;
                    string entrymessage = Locale.GetLocale(skuser.Language).GetString("SKULD_SEARCH_WEEB_MKSLCTN") +" "+timeout+"s\n";
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
                            if (entries.LastOrDefault() == entries[x-1])
                                tempstring += clonedarr[x-1];
                            else
                                tempstring += clonedarr[x-1] + "\n\n";
                            if (x % 10 == 0)
                            { pages.Add(tempstring); tempstring = ""; }
                            else if (x == entries.Count())
                            { pages.Add(tempstring); tempstring = ""; }
                            Console.WriteLine(tempstring);
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
                        msg = await MessageHandler.SendChannel(Context.Channel, entrymessage);
                    }
                    var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout: TimeSpan.FromSeconds(timeout));
                    await GetAnimeAtPosition(Convert.ToInt32(response.Content), response.Channel);

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
                    await GetAnimeAtZeroNonSearch();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        public async Task GetAnimeAtZeroNonSearch() =>
            await SendAnime(AnimeArray.Entry.First());
        private async Task GetAnimeAtPosition(int position, IMessageChannel channel) =>
            await SendAnime(AnimeArray.Entry[position - 1], channel);
        private async Task SendAnime(Anime animu, IMessageChannel channel = null) =>
            await MessageHandler.SendChannel(channel ?? Context.Channel, "", BuildMALEmbed(animu, Locale.GetLocale((await SqlTools.GetUserAsync(Context.User.Id)).Language)));

        private static Embed BuildMALEmbed(Anime animu, System.Resources.ResourceManager userlocale)
        {
            var embed = new EmbedBuilder
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Url = $"https://myanimelist.net/anime/{animu.Id}/",
                    Name = animu.Title
                },
                ThumbnailUrl = animu.Image
            };
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_ENGTITLE"), animu.EnglishTitle ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SYNON"), animu.Synonyms ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_EPS"), animu.Episodes ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SDATE"), animu.StartDate ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_EDATE"), animu.EndDate ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SCORE"), animu.Score ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SYNOP"), HttpUtility.HtmlDecode(animu.Synopsis.Split('<')[0]) ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            return embed.Build();
        }

        private static Embed BuildMALEmbed(Manga mango, System.Resources.ResourceManager userlocale)
        {
            var embed = new EmbedBuilder()
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Url = $"https://myanimelist.net/manga/{mango.Id}/",
                    Name = mango.Title
                },
                ThumbnailUrl = mango.Image
            };
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_ENGTITLE"), mango.EnglishTitle ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SYNON"), mango.Synonyms ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_CHPS"), mango.Chapters ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_VOLS"), mango.Volumes ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SDATE"), mango.StartDate ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_EDATE"), mango.EndDate ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SCORE"), mango.Score ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_SYNOP"), HttpUtility.HtmlDecode(mango.Synopsis.Split('<')[0]) ?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            return embed.Build();
        }

        [Command("weebgif", RunMode = RunMode.Async), Summary("Gets a weeb gif")]
        public async Task WeebGif()
        {
            await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder()
            {
                ImageUrl = await APIWebReq.ReturnString(new Uri("http://lucoa.systemexit.co.uk/gifs/reactions/")),
                Color = Tools.Tools.RandomColor()
            });
        }
    }
}
