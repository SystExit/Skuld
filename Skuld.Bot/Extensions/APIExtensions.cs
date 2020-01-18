using Akitaux.Twitch.Helix;
using Akitaux.Twitch.Helix.Entities;
using Akitaux.Twitch.Helix.Requests;
using Discord;
using Kitsu.Anime;
using Kitsu.Manga;
using Skuld.APIS.Extensions;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Extensions;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using SteamStoreQuery;
using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Voltaic;

namespace Skuld.Bot.Extensions
{
    public static class APIExtensions
    {
        public static EmbedBuilder ToEmbed(this AnimeDataModel model, ResourceManager loc)
        {
            var attr = model.Attributes;

            var embed = new EmbedBuilder
            {
                Title = attr.CanonicalTitle.CheckEmptyWithLocale(loc),
                ImageUrl = attr.PosterImage.Large
            };

            if (attr.AbbreviatedTitles != null && attr.AbbreviatedTitles.Any())
                embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNON"), attr.AbbreviatedTitles.CheckEmptyWithLocale(", ", loc));

            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EPS"), attr.EpisodeCount.CheckEmptyWithLocale(loc));
            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SDATE"), attr.StartDate.CheckEmptyWithLocale(loc));
            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EDATE"), attr.EndDate.CheckEmptyWithLocale(loc));
            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SCORE"), attr.RatingRank.CheckEmptyWithLocale(loc));

            if (attr.Synopsis.CheckEmptyWithLocale(loc) != loc.GetString("SKULD_GENERIC_EMPTY"))
            {
                var syno = attr.Synopsis;
                if (syno.Length > 1024)
                    embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis.Substring(0, 1021) + "...");
                else
                    embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis);
            }

            embed.WithColor(Color.Purple);

            return embed;
        }

        public static EmbedBuilder ToEmbed(this MangaDataModel model, ResourceManager loc)
        {
            var attr = model.Attributes;

            var embed = new EmbedBuilder
            {
                Title = attr.CanonicalTitle.CheckEmptyWithLocale(loc),
                ImageUrl = attr.PosterImage.Large
            };

            if (attr.AbbreviatedTitles != null && attr.AbbreviatedTitles.Any())
                embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNON"), attr.AbbreviatedTitles.CheckEmptyWithLocale(", ", loc));

            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EPS"), attr.ChapterCount.CheckEmptyWithLocale(loc));
            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SDATE"), attr.StartDate.CheckEmptyWithLocale(loc));
            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EDATE"), attr.EndDate.CheckEmptyWithLocale(loc));
            embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SCORE"), attr.RatingRank.CheckEmptyWithLocale(loc));

            if (attr.Synopsis.CheckEmptyWithLocale(loc) != loc.GetString("SKULD_GENERIC_EMPTY"))
            {
                var syno = attr.Synopsis;
                if (syno.Length > 1024)
                    embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis.Substring(0, 1021) + "...");
                else
                    embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis);
            }

            embed.WithColor(Color.Purple);

            return embed;
        }

        public static async Task<EmbedBuilder> GetEmbedAsync(this Listing game)
        {
            var SteamStore = new SteamWebAPI2.Interfaces.SteamStore();
            var app = await SteamStore.GetStoreAppDetailsAsync(Convert.ToUInt32(game.AppId));

            string releasetext = app.ReleaseDate.ComingSoon ? "Coming soon! (Date Number may not be accurate)" : "Released on:";

            var embed =
                new EmbedBuilder()
                    .WithAuthor(
                        new EmbedAuthorBuilder()
                        .WithName(string.Join(", ", app.Developers))
                    )
                    .WithDescription(SteamUtilities.GetSteamGameDescription(game, app))
                    .WithImageUrl(app.Screenshots.Random().PathFull)
                    .WithRandomColor()
                    .WithUrl($"https://store.steampowered.com/app/{game.AppId}/")
                    .WithTitle(game.Name)
                    .WithFooter(
                        new EmbedFooterBuilder()
                        .WithText(releasetext)
                    );

            if (int.TryParse(app.ReleaseDate.Date[0].ToString(), out int _))
            {
                embed.WithTimestamp(DateTime.ParseExact(app.ReleaseDate.Date, "dd MMM, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces));
            }
            else
            {
                embed.WithTimestamp(DateTime.ParseExact(app.ReleaseDate.Date, "MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces));
            }

            return embed;
        }

        public async static Task<Stream> GetStreamAsync(this User channel, TwitchHelixClient client)
        {
            var stream = await client.GetStreamsAsync(new GetStreamsParams
            {
                UserNames = new[] { channel.Name.Value }
            });

            return stream.Data.FirstOrDefault();
        }

        public async static Task<EmbedBuilder> GetEmbedAsync(this User channel, TwitchHelixClient client)
        {
            var name = channel.DisplayName.IsSpecified ? channel.DisplayName.Value : (channel.Name.IsSpecified ? channel.Name.Value : new Utf8String($"{channel.Id}"));
            var iconurl = channel.ProfileImageUrl.IsSpecified ? channel.ProfileImageUrl.Value : new Utf8String("");

            string twitchStatus = "";
            string channelIcon = "";

            switch (channel.Type.Value)
            {
                case UserType.Staff:
                    twitchStatus = DiscordUtilities.TwitchStaff.ToString();
                    break;

                case UserType.Admin:
                    twitchStatus = DiscordUtilities.TwitchAdmins.ToString();
                    break;

                case UserType.GlobalMod:
                    twitchStatus = DiscordUtilities.TwitchGlobalMod.ToString();
                    break;
            }

            switch (channel.BroadcasterType.Value)
            {
                case BroadcasterType.Partner:
                    channelIcon = DiscordUtilities.TwitchVerified.ToString();
                    break;

                case BroadcasterType.Affiliate:
                    channelIcon = DiscordUtilities.TwitchAffiliate.ToString();
                    break;
            }

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = (string)name,
                    IconUrl = (string)iconurl,
                    Url = "https://twitch.tv/" + channel.Name
                },
                ThumbnailUrl = (string)iconurl,
                Color = EmbedExtensions.RandomEmbedColor()
            };

            string channelBadges = null;

            if (twitchStatus != null)
            {
                channelBadges += twitchStatus;
            }
            if (channelIcon != null)
            {
                channelBadges += channelIcon;
            }

            if (channelBadges != null)
            {
                embed.AddInlineField("Channel Badges", channelBadges);
            }

            var stream = await channel.GetStreamAsync(client);

            if (stream != null)
            {
                embed.Title = (string)(stream.Title.IsSpecified ? stream.Title.Value : new Utf8String("No Title Set"));

                if (stream.GameId.IsSpecified)
                {
                    /*var resp = await client.GetGamesAsync(new GetGamesParams
                    {
                        GameIds = new[] { stream.GameId.Value }
                    });
                    embed.AddInlineField("Playing", resp.Data.FirstOrDefault().Name);*/
                }
                embed.AddInlineField("Viewers 👀👀", $"{((stream.ViewerCount.IsSpecified ? stream.ViewerCount.Value : -1) > -1 ? stream.ViewerCount.Value.ToString("N0") : "N/A")}");

                if (stream.ThumbnailUrl.IsSpecified)
                {
                    string bigimg = Convert.ToString(stream.ThumbnailUrl.Value);
                    bigimg = bigimg.Replace("{width}", "1280").Replace("{height}", "720");
                    embed.ImageUrl = bigimg;
                }

                embed.AddInlineField("Started streaming", stream.StartedAt.Value.ToString("dd/MM/yyyy HH:mm:ss"));

                var uptime = DateTime.UtcNow.Subtract(stream.StartedAt.Value);

                string uptimeString = "";

                if (uptime.Days > 0)
                {
                    uptimeString += $"{uptime.Days} days ";
                }
                if (uptime.Hours > 0)
                {
                    uptimeString += $"{uptime.Hours} hours ";
                }
                if (uptime.Minutes > 0)
                {
                    uptimeString += $"{uptime.Minutes} minutes ";
                }
                if (uptime.Seconds > 0)
                {
                    uptimeString += $"{uptime.Minutes} seconds ";
                }

                embed.AddInlineField("Uptime", $"{uptimeString.Substring(0, uptimeString.Count() - 1)}");
            }
            else
            {
                embed.AddInlineField("Total Views", channel.TotalViews.Value.ToString("N0"));
            }

            return embed;
        }

        public static EmbedBuilder ToEmbed(this UrbanWord word)
        {
            return
                new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithUrl(word.PermaLink)
                    .WithName(word.Word)
                )
                .WithDescription(word.Definition)
                .WithRandomColor()
                .AddField("Author", word.Author)
                .AddField("Example", word.Example)
                .AddField("Upvotes", word.UpVotes)
                .AddInlineField("Downvotes", word.DownVotes);
        }
    }
}