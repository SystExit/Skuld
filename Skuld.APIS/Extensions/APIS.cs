using Booru.Net;
using Discord;
using Imgur.API.Models;
using Kitsu.Anime;
using Kitsu.Manga;
using Akitaux.Twitch.Helix;
using Akitaux.Twitch.Helix.Entities;
using Akitaux.Twitch.Helix.Requests;
using Skuld.APIS.Pokemon.Models;
using Skuld.APIS.Social.Reddit.Models;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Steam.Models.SteamStore;
using SteamStoreQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Voltaic;

namespace Skuld.APIS.Extensions
{
    public static class APIS
    {
        private static Random rnd = new Random(DateTime.UtcNow.Millisecond);
        private static List<string> blacklistedTags = new List<string>
        {
            "loli",
            "shota",
            "gore",
            "vore",
            "death"
        };

        public static DanbooruImage GetRandomImage(this IReadOnlyList<DanbooruImage> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static Rule34Image GetRandomImage(this IReadOnlyList<Rule34Image> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static RealbooruImage GetRandomImage(this IReadOnlyList<RealbooruImage> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static SafebooruImage GetRandomImage(this IReadOnlyList<SafebooruImage> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static GelbooruImage GetRandomImage(this IReadOnlyList<GelbooruImage> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static KonaChanImage GetRandomImage(this IReadOnlyList<KonaChanImage> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static E621Image GetRandomImage(this IReadOnlyList<E621Image> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static YandereImage GetRandomImage(this IReadOnlyList<YandereImage> posts)
            => posts[rnd.Next(0, posts.Count)];

        public static T GetRandomEntry<T>(this IEnumerable<T> entries)
            => entries.ElementAtOrDefault(rnd.Next(0, entries.Count()));

        public static string GetMessage(this BooruImage image, string postUrl)
        {
            string message = $"`Score: {image.Score}` <{postUrl}>\n{image.ImageUrl}";

            if (image.ImageUrl.IsVideoFile())
            {
                message += " (Video)";
            }

            return message;
        }

        public static string GetMessage(this SafebooruImage image, string postUrl)
        {
            string message = $"`Score: {image.Score}` <{postUrl}>\n{image.ImageUrl}";

            if (image.ImageUrl.IsVideoFile())
            {
                message += " (Video)";
            }

            return message;
        }

        public static IGalleryItem GetRandomItem(this IEnumerable<IGalleryItem> list)
            => list.ElementAtOrDefault(rnd.Next(0, list.Count()));

        public static StoreScreenshotModel Random(this IReadOnlyList<StoreScreenshotModel> elements)
            => elements[rnd.Next(0, elements.Count)];

        public static IList<string> PaginateList(this IReadOnlyList<AnimeDataModel> list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"{x + 1}. {obj.Attributes.CanonicalTitle}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginateList(this IReadOnlyList<MangaDataModel> list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"{x + 1}. {obj.Attributes.CanonicalTitle}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginateList(this IReadOnlyList<Listing> list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"{x + 1}. {obj.Name}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginatePosts(this Post[] posts, ITextChannel channel)
        {
            var Pages = new List<string>();

            string pagetext = "";

            for (int x = 0; x < posts.Length; x++)
            {
                var post = posts[x];

                string txt = $"[{post.Data.Title}](https://reddit.com{post.Data.Permalink})\n";

                if (post.Data.Over18 && channel.IsNsfw)
                {
                    pagetext += "**NSFW** " + txt;
                }
                else
                {
                    pagetext += txt;
                }
                pagetext += "\n";

                if ((x + 1) % 10 == 0 || (x + 1) == posts.Length)
                {
                    Pages.Add(pagetext);
                    pagetext = "";
                }
            }
            return Pages;
        }

        public static IList<string> AddBlacklistedTags(this IList<string> tags)
        {
            var newtags = new List<string>();
            newtags.AddRange(tags);
            blacklistedTags.ForEach(x => newtags.Add("-" + x));
            return newtags;
        }

        public static bool ContainsBlacklistedTags(this string[] tags)
        {
            bool returnvalue = false;
            foreach (var tag in tags)
            {
                if (blacklistedTags.Contains(tag.ToLowerInvariant()))
                {
                    returnvalue = true;
                }
            }
            return returnvalue;
        }

        public static Embed ToEmbed(this AnimeDataModel model, ResourceManager loc)
        {
            var attr = model.Attributes;

            var embed = new EmbedBuilder
            {
                Title = attr.CanonicalTitle.CheckEmptyWithLocale(loc),
                ImageUrl = attr.PosterImage.Large
            };

            if (attr.AbbreviatedTitles != null && attr.AbbreviatedTitles.Count() != 0)
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

            if (attr.AbbreviatedTitles != null && attr.AbbreviatedTitles.Count() != 0)
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

            string releasetext = "";
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

        public async static Task<bool> IsStreamingAsync(this User channel, TwitchHelixClient client)
        {
            var stream = await client.GetStreamsAsync(new GetStreamsParams
            {
                UserIds = new[]{(ulong)channel.Id}
            });

            var resp = stream.Data.FirstOrDefault();

            bool result = false;

            if(resp.StartedAt.IsSpecified)
            {
                result = true;
            }

            return result;
        }

        public async static Task<Stream> GetStreamAsync(this User channel, TwitchHelixClient client)
        {
            var stream = await client.GetStreamsAsync(new GetStreamsParams
            {
                UserIds = new[] { (ulong)channel.Id }
            });

            return stream.Data.FirstOrDefault();
        }

        public async static Task<Embed> GetEmbedAsync(this User channel, TwitchHelixClient client)
        {
            if (await channel.IsStreamingAsync(client))
            {
                var name = channel.DisplayName.IsSpecified ? channel.DisplayName.Value : (channel.Name.IsSpecified ? channel.Name.Value : new Utf8String($"{channel.Id}"));
                var iconurl = channel.ProfileImageUrl.IsSpecified ? channel.ProfileImageUrl.Value : new Utf8String("");

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

                var stream = await channel.GetStreamAsync(client);

                embed.Title = (string)(stream.Title.IsSpecified ? stream.Title.Value : new Utf8String("No Title Set"));

                if (stream.GameId.IsSpecified)
                {
                    var resp = await client.GetGamesAsync(new GetGamesParams
                    {
                        GameIds = new[] { (ulong)stream.GameId }
                    });
                    embed.AddField("Playing", resp.Data.FirstOrDefault().Name, true);
                }
                embed.AddField("For", $"{((stream.ViewerCount.IsSpecified ? stream.ViewerCount.Value : -1) > -1 ? ((int)stream.ViewerCount.Value).ToString("N0") : "N/A")} viewers", true);

                if (stream.ThumbnailUrl.IsSpecified)
                {
                    embed.ImageUrl = (string)stream.ThumbnailUrl.Value;
                }

                return embed.Build();
            }
            return null;
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