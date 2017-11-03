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
        private MangaArr MangaArray;
        [Command("manga", RunMode = RunMode.Async), Summary("Gets a manga from MyAnimeList.Net")]
        public async Task MangaGet([Remainder]string mangatitle)
        {
			MangaArray = await MALAPI.GetMangasAsync(mangatitle);
            int manganode = MangaArray.Entry.Count;
            if (manganode > 1)
            {
                IMessage msg = null;
                var msgs = new List<IMessage>();
                string entrymessage = "__Make your selection__ Make your selection within 10 seconds\n";
                int count = 0;
                var entries = new List<string>();
                foreach (var item in MangaArray.Entry)
                {
                    count++;
                    entries.Add($"**{count}. {item.Title}**");
                }
                if (entries.Count >= 20)
                {
                    entrymessage += String.Join(Environment.NewLine, entries.Take(entries.Count / 2).ToArray());
                    msgs.Add(await MessageHandler.SendChannel(Context.Channel, entrymessage));
                    entrymessage = "";
                    entrymessage += String.Join(Environment.NewLine, entries.Skip(entries.Count / 2).ToArray());
                    msgs.Add(await MessageHandler.SendChannel(Context.Channel, entrymessage));
                }
                else
                {
                    entrymessage += String.Join(Environment.NewLine, entries.ToArray());
                    msg = await MessageHandler.SendChannel(Context.Channel, entrymessage);
                }
                var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout:TimeSpan.FromSeconds(10));
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

        public async Task SendManga(Manga mango, IMessageChannel channel = null)
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

			var userlocale = Locale.GetLocale((await SqlTools.GetUserAsync(Context.User.Id)).Language);
			embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_ENGTITLE"), mango.EnglishTitle??userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			embed.AddField("SKULD_SEARCH_WEEB_SYNON", mango.Synonyms?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			embed.AddField("SKULD_SEARCH_WEEB_CHPS", mango.Chapters?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			embed.AddField("SKULD_SEARCH_WEEB_VOLS", mango.Volumes?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			embed.AddField("SKULD_SEARCH_WEEB_SDATE", mango.StartDate?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			embed.AddField("SKULD_SEARCH_WEEB_EDATE", mango.EndDate?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			embed.AddField("SKULD_SEARCH_WEEB_SCORE", mango.Score?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			embed.AddField("SKULD_SEARCH_WEEB_SYNOP", HttpUtility.HtmlDecode(mango.Synopsis.Split('<')[0])?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
			await MessageHandler.SendChannel(channel ?? Context.Channel, "", embed);
        }

        private AnimeArr AnimeArray;
        [Command("anime", RunMode = RunMode.Async), Summary("Gets an anime from MyAnimeList.Net")]
        public async Task Animuget([Remainder]string animetitle)
        {
			var skuser = await SqlTools.GetUserAsync(Context.User.Id);
			var pages = new List<string>();
			AnimeArray = await MALAPI.GetAnimesAsync(animetitle);
            int animenodes = AnimeArray.Entry.Count;
			if (animenodes == 0)
				await ReplyAsync(Locale.GetLocale(skuser.Language).GetString("SKULD_SEARCH_WEEB_EMPTY"));
            if (animenodes > 1)
			{
				IMessage msg = null;
				string entrymessage = "Make your selection within 20 seconds\n";
                int count = 0;
                var entries = new List<string>();
                foreach (var item in AnimeArray.Entry)
                {
                    count++;
                    entries.Add($"**{count}. {item.Title}**");
                }
                if (entries.Count >= 20)
                {
					string tempstring = "";
					for (int x = 0; x < entries.Count; x++)
					{
						if (entries.LastOrDefault() == entries[x])
							tempstring += entries[x];
						else
							tempstring += entries[x]+"\n\n";
						if (x % 10 == 0)
							pages.Add(tempstring);
						else if (x == entries.Count())
							pages.Add(tempstring);
					}
					var pagedMessage = new PaginatedMessage(){
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
                var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout: TimeSpan.FromSeconds(10));
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
        public async Task GetAnimeAtZeroNonSearch() =>
            await SendAnime(AnimeArray.Entry.First());
        private async Task GetAnimeAtPosition(int position, IMessageChannel channel) =>
            await SendAnime(AnimeArray.Entry[position - 1], channel);
        private async Task SendAnime(Anime animu, IMessageChannel channel = null)
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
			var userlocale = Locale.GetLocale((await SqlTools.GetUserAsync(Context.User.Id)).Language);
            embed.AddField(userlocale.GetString("SKULD_SEARCH_WEEB_ENGTITLE"), animu.EnglishTitle?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField("SKULD_SEARCH_WEEB_SYNON", animu.Synonyms?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField("SKULD_SEARCH_WEEB_EPS", animu.Episodes?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField("SKULD_SEARCH_WEEB_SDATE", animu.StartDate?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField("SKULD_SEARCH_WEEB_EDATE", animu.EndDate?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField("SKULD_SEARCH_WEEB_SCORE", animu.Score?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);
            embed.AddField("SKULD_SEARCH_WEEB_SYNOP", HttpUtility.HtmlDecode(animu.Synopsis.Split('<')[0])?? userlocale.GetString("SKULD_GENERIC_EMPTY"), inline: true);

            await MessageHandler.SendChannel(channel ?? Context.Channel, "", embed);
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
