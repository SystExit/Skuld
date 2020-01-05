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
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Voltaic;

namespace Skuld.Bot.Extensions
{
    public static class APIExtensions
    {
        public static Embed ToEmbed(this AnimeDataModel model, ResourceManager loc)
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

            return embed.Build();
        }

        public static Embed ToEmbed(this MangaDataModel model, ResourceManager loc)
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

            return embed.Build();
        }

        public static async Task<Embed> GetEmbedAsync(this Listing game)
        {
            var SteamStore = new SteamWebAPI2.Interfaces.SteamStore();
            var app = await SteamStore.GetStoreAppDetailsAsync(Convert.ToUInt32(game.AppId));

            string releasetext;
            if (app.ReleaseDate.ComingSoon)
            {
                releasetext = $"Coming soon! ({app.ReleaseDate.Date})";
            }
            else
            {
                releasetext = $"Released on: {app.ReleaseDate.Date}";
            }

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = String.Join(", ", app.Developers)
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = releasetext
                },
                Description = SteamUtilities.GetSteamGameDescription(game, app),
                Title = game.Name,
                ImageUrl = app.Screenshots.Random().PathFull,
                Url = $"https://store.steampowered.com/app/{game.AppId}/",
                Color = EmbedUtils.RandomColor()
            };

            return embed.Build();
        }

        public async static Task<Stream> GetStreamAsync(this User channel, TwitchHelixClient client)
        {
            var stream = await client.GetStreamsAsync(new GetStreamsParams
            {
                UserNames = new[] { channel.Name.Value }
            });

            return stream.Data.FirstOrDefault();
        }

        public async static Task<Embed> GetEmbedAsync(this User channel, TwitchHelixClient client)
        {
            var name = channel.DisplayName.IsSpecified ? channel.DisplayName.Value : (channel.Name.IsSpecified ? channel.Name.Value : new Utf8String($"{channel.Id}"));
            var iconurl = channel.ProfileImageUrl.IsSpecified ? channel.ProfileImageUrl.Value : new Utf8String("");

            string twitchStatus = "";
            string channelIcon = "";

            switch (channel.Type.Value)
            {
                case UserType.Staff:
                    twitchStatus = DiscordTools.TwitchStaff;
                    break;

                case UserType.Admin:
                    twitchStatus = DiscordTools.TwitchAdmins;
                    break;

                case UserType.GlobalMod:
                    twitchStatus = DiscordTools.TwitchGlobalMod;
                    break;
            }

            switch (channel.BroadcasterType.Value)
            {
                case BroadcasterType.Partner:
                    channelIcon = DiscordTools.TwitchVerified;
                    break;

                case BroadcasterType.Affiliate:
                    channelIcon = DiscordTools.TwitchAffiliate;
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
                Color = EmbedUtils.RandomColor()
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

            return embed.Build();
        }

        public static Embed ToEmbed(this UrbanWord word)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = word.Word,
                    Url = word.PermaLink
                },
                Description = word.Definition,
                Color = EmbedUtils.RandomColor()
            };
            embed.AddField("Author", word.Author);
            embed.AddField("Example", word.Example);
            embed.AddField("Upvotes", word.UpVotes);
            embed.AddInlineField("Downvotes", word.DownVotes);
            return embed.Build();
        }
    }
}
