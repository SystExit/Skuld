using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using Skuld.Services;
using SysEx.Net;
using Kitsu.Anime;
using Kitsu.Manga;
using Skuld.Extensions;
using Skuld.Globalization;
using System;
using Skuld.Utilities;

namespace Skuld.Modules
{
    [Group]
    public class Weeb : InteractiveBase<ShardedCommandContext>
    {
        public DatabaseService Database { get; set; }
        public Locale Locale { get; set; }
        public SysExClient SysExClient { get; set; }
        public LoggingService Logger { get; set; }

        [Command("anime"), Summary("Gets information about an anime")]
        public async Task GetAnime([Remainder]string animetitle)
        {
            var usr = await Database.GetUserAsync(Context.User.Id);
            var loc = Locale.GetLocale(Locale.defaultLocale);
            if (usr != null)
                loc = Locale.GetLocale(usr.Language);

            var raw = await Anime.GetAnimeAsync(animetitle);
            var data = raw.Data;
            if(data.Count > 1) // do pagination
            {
                var pages = data.PaginateList();

                IUserMessage sentmessage = null;

                if(pages.Count>1)
                {
                    sentmessage = await PagedReplyAsync(new PaginatedMessage
                    {
                        Title = loc.GetString("SKULD_SEARCH_WEEB_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Pages = pages
                    }, true);

                }
                else
                {
                    sentmessage = await Context.Channel.ReplyAsync(new EmbedBuilder
                    {
                        Title = loc.GetString("SKULD_SEARCH_WEEB_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Description = pages[0]
                    }.Build());
                }

                var responce = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                if(responce == null)
                {
                    await sentmessage.DeleteAsync();
                }

                var selection = Convert.ToInt32(responce.Content);

                var anime = data[selection-1];

                await Context.Channel.ReplyAsync(anime.ToEmbed(loc));
            }
            else
            {
                var anime = data[0];

                await Context.Channel.ReplyAsync(anime.ToEmbed(loc));
            }
        }

        [Command("manga"), Summary("Gets information about a manga")]
        public async Task GetMangaAsync([Remainder]string mangatitle)
        {
            var usr = await Database.GetUserAsync(Context.User.Id);
            var loc = Locale.GetLocale(Locale.defaultLocale);
            if (usr != null)
                loc = Locale.GetLocale(usr.Language);

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
                        Title = loc.GetString("SKULD_SEARCH_WEEB_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Pages = pages
                    }, true);

                }
                else
                {
                    sentmessage = await Context.Channel.ReplyAsync(new EmbedBuilder
                    {
                        Title = loc.GetString("SKULD_SEARCH_WEEB_MKSLCTN") + " 30s",
                        Color = Color.Purple,
                        Description = pages[0]
                    }.Build());
                }

                var responce = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                if (responce == null)
                {
                    await sentmessage.DeleteAsync();
                }

                var selection = Convert.ToInt32(responce.Content);

                var manga = data[selection - 1];

                await Context.Channel.ReplyAsync(manga.ToEmbed(loc));
            }
            else
            {
                var manga = data[0];

                await Context.Channel.ReplyAsync(manga.ToEmbed(loc));
            }
        }

        [Command("weebgif"), Summary("Gets a weeb gif")]
        public async Task WeebGif()
        {
            var gif = await SysExClient.GetWeebReactionGifAsync();

            var embed = new EmbedBuilder
            {
                ImageUrl = gif.URL,
                Color = EmbedUtils.RandomColor()
            };

            await Context.Channel.ReplyAsync(embed.Build());
        }
    }
}