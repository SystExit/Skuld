using Booru.Net;
using Discord;
using Imgur.API.Models;
using Kitsu.Anime;
using Kitsu.Manga;
using Microsoft.Extensions.DependencyInjection;
using NTwitch.Rest;
using Skuld.APIS.Pokemon.Models;
using Skuld.APIS.Social.Reddit.Models;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Extensions;
using Skuld.Core.Utilities;
using Skuld.Services;
using Skuld.Utilities.Discord;
using Steam.Models.SteamStore;
using SteamStoreQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace Skuld.Extensions
{
    public static class APIS
    {
        private static List<string> blacklistedTags = new List<string>
        {
            "loli",
            "shota",
            "gore",
            "vore",
            "death"
        };

        public static DanbooruImage GetRandomImage(this IReadOnlyList<DanbooruImage> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static Rule34Image GetRandomImage(this IReadOnlyList<Rule34Image> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static RealbooruImage GetRandomImage(this IReadOnlyList<RealbooruImage> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static SafebooruImage GetRandomImage(this IReadOnlyList<SafebooruImage> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static GelbooruImage GetRandomImage(this IReadOnlyList<GelbooruImage> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static KonaChanImage GetRandomImage(this IReadOnlyList<KonaChanImage> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static E621Image GetRandomImage(this IReadOnlyList<E621Image> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static YandereImage GetRandomImage(this IReadOnlyList<YandereImage> posts)
            => posts[HostService.Services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static string GetMessage(this BooruImage image, string postUrl)
        {
            string message = $"`Score: {image.Score}` <{postUrl}>\n{image.ImageUrl}";

            if (image.ImageUrl.IsVideoFile())
            {
                message += "(Video)";
            }

            return message;
        }

        public static string GetMessage(this SafebooruImage image, string postUrl)
        {
            string message = $"`Score: {image.Score}` <{postUrl}>\n{image.ImageUrl}";

            if (image.ImageUrl.IsVideoFile())
            {
                message += "(Video)";
            }

            return message;
        }

        public static IGalleryItem GetRandomItem(this IEnumerable<IGalleryItem> list)
            => list.ElementAtOrDefault(HostService.Services.GetRequiredService<Random>().Next(0, list.Count()));

        public static StoreScreenshotModel Random(this IReadOnlyList<StoreScreenshotModel> elements)
            => elements[HostService.Services.GetRequiredService<Random>().Next(0, elements.Count)];

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

            if(attr.AbbreviatedTitles != null && attr.AbbreviatedTitles.Count() != 0)
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
                Url = game.StoreLink,
                Color = EmbedUtils.RandomColor()
            };

            return embed.Build();
        }

        public async static Task<bool> IsStreamingAsync(this RestChannel channel)
        {
            var stream = await channel.GetStreamAsync();

            if (stream == null) return false;

            return true;
        }

        public async static Task<Embed> GetEmbedAsync(this RestChannel channel)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = channel.DisplayName,
                    IconUrl = channel.LogoUrl,
                    Url = channel.Url
                },
                ThumbnailUrl = channel.LogoUrl,
                Color = ConversionTools.HexToDiscordColor(channel.ProfileBannerBackgroundColor)
            };

            if (await channel.IsStreamingAsync())
            {
                var stream = await channel.GetStreamAsync();
                embed.Title = channel.Status ?? "No Title Set";
                embed.AddField("Playing", channel.Game, true);
                embed.AddField("For", $"{stream.Viewers.ToString("N0")} viewers", true);
                embed.ImageUrl = stream.Previews.SkipWhile(x => x.Key != "large").FirstOrDefault().Value;
            }
            else
            {
                if (!String.IsNullOrEmpty(channel.Status))
                {
                    embed.AddField("Last stream title", channel.Status, true);
                }
                else
                {
                    embed.AddField("Last stream title", "Unset", true);
                }
                if (!String.IsNullOrEmpty(channel.Game))
                {
                    embed.AddField("Was last streaming", channel.Game, true);
                }
                else
                {
                    embed.AddField("Was last streaming", "Nothing", true);
                }
                embed.AddField("Followers", $"{channel.Followers.ToString("N0")}", true);
                embed.AddField("Total Views", $"{channel.Views.ToString("N0")}", true);

                embed.WithThumbnailUrl(channel.VideoBannerUrl);
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

        public static Embed GetEmbed(this PokeSharp.Models.PocketMonster pokemon, PokeSharpGroup group)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = char.ToUpper(pokemon.Name[0]) + pokemon.Name.Substring(1)
                },
                Color = Color.Blue
            };

            var result = HostService.Services.GetRequiredService<Random>().Next(0, 8193);
            string sprite = null;
            //if it equals 8 out of a random integer between 1 and 8192 then give shiny
            if (result == 8)
            {
                sprite = pokemon.Sprites.FrontShiny;
            }
            else
            {
                sprite = pokemon.Sprites.Front;
            }

            switch (group)
            {
                case PokeSharpGroup.Default:
                    embed.AddInlineField("Height", pokemon.Height + "mm");
                    embed.AddInlineField("Weight", pokemon.Weight + "kg");
                    embed.AddInlineField("ID", pokemon.ID.ToString());
                    embed.AddInlineField("Base Experience", pokemon.BaseExperience.ToString());
                    break;

                case PokeSharpGroup.Abilities:
                    foreach (var ability in pokemon.Abilities)
                    {
                        embed.AddInlineField(ability.Ability.Name, "Slot: " + ability.Slot);
                    }
                    break;

                case PokeSharpGroup.Games:
                    string games = null;
                    foreach (var game in pokemon.GameIndices)
                    {
                        games += game.Version.Name + "\n";
                        if (game == pokemon.GameIndices.Last())
                        {
                            games += game.Version.Name;
                        }
                    }
                    embed.AddInlineField("Game", games);
                    break;

                case PokeSharpGroup.HeldItems:
                    if (pokemon.HeldItems.Length > 0)
                    {
                        foreach (var hitem in pokemon.HeldItems)
                        {
                            foreach (var game in hitem.VersionDetails)
                            {
                                embed.AddInlineField("Item", hitem.Item.Name + "\n**Game**\n" + game.Version.Name + "\n**Rarity**\n" + game.Rarity);
                            }
                        }
                    }
                    else
                    {
                        embed.Description = "This pokemon doesn't hold any items in the wild";
                    }
                    break;

                case PokeSharpGroup.Moves:
                    var moves = pokemon.Moves.Take(4).Select(i => i).ToArray();
                    foreach (var move in moves)
                    {
                        string mve = move.Move.Name;
                        mve += "\n**Learned at:**\n" + "Level " + move.VersionGroupDetails.FirstOrDefault().LevelLearnedAt;
                        mve += "\n**Method:**\n" + move.VersionGroupDetails.FirstOrDefault().MoveLearnMethod.Name;
                        embed.AddInlineField("Move", mve);
                    }
                    embed.Author.Url = "https://bulbapedia.bulbagarden.net/wiki/" + pokemon.Name + "_(Pokémon)";
                    embed.Footer = new EmbedFooterBuilder { Text = "Click the name to view more moves, I limited it to 4 to prevent a wall of text" };
                    break;

                case PokeSharpGroup.Stats:
                    foreach (var stat in pokemon.Stats)
                    {
                        embed.AddInlineField(stat.Stat.Name, "Base Stat: " + stat.BaseStat);
                    }
                    break;
            }
            embed.ThumbnailUrl = sprite;

            return embed.Build();
        }
    }
}