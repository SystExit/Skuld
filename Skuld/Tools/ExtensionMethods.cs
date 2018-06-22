using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Booru.Net;
using Steam.Models.SteamStore;
using HtmlAgilityPack;
using System.Text;
using System.IO;
using Kitsu.Anime;
using Discord;
using System.Threading.Tasks;
using Skuld.Services;
using Discord.WebSocket;
using System.Resources;
using Kitsu.Manga;
using NTwitch.Rest;
using System.Linq;
using SteamStoreQuery;
using Imgur.API.Models;
using Skuld.Models.API;
using Skuld.Models;
using Skuld.Utilities;

namespace Skuld.Extensions
{
    public static class ExtensionMethods
    {
        private static readonly string[] VideoExtensions = {
            ".webm",
            ".mkv",
            ".flv",
            ".vob",
            ".ogv",
            ".ogg",
            ".avi",
            ".mov",
            ".qt",
            ".wmv",
            ".mp4",
            ".m4v",
            ".mpg",
            ".mpeg"
        };
        private static readonly string[] ImageExtensions = 
        {
            ".jpg",
            ".bmp",
            ".gif",
            ".png"
        };

        static List<string> blacklistedTags = new List<string>
        {
            "loli",
            "shota",
            "gore",
            "vore",
            "death"
        };

        public static LogSeverity ToDiscord(this NTwitch.LogSeverity logSeverity)
		{
			if (logSeverity == NTwitch.LogSeverity.Critical)
				return Discord.LogSeverity.Critical;
			if (logSeverity == NTwitch.LogSeverity.Debug)
				return Discord.LogSeverity.Debug;
			if (logSeverity == NTwitch.LogSeverity.Error)
				return Discord.LogSeverity.Error;
			if (logSeverity == NTwitch.LogSeverity.Info)
				return Discord.LogSeverity.Info;
			if (logSeverity == NTwitch.LogSeverity.Verbose)
				return Discord.LogSeverity.Verbose;
			if (logSeverity == NTwitch.LogSeverity.Warning)
				return Discord.LogSeverity.Warning;

			return Discord.LogSeverity.Verbose;
		}

        public static ConsoleColor SeverityToColor(this LogSeverity sev)
        {
            if (sev == LogSeverity.Critical)
                return ConsoleColor.DarkRed;
            if (sev == LogSeverity.Error)
                return ConsoleColor.Red;
            if (sev == LogSeverity.Info)
                return ConsoleColor.Green;
            if (sev == LogSeverity.Warning)
                return ConsoleColor.Yellow;
            if (sev == LogSeverity.Verbose)
                return ConsoleColor.Cyan;
            return ConsoleColor.White;
        }

        public static bool ToBool(this string data)
        {
            if (data.ToLowerInvariant() == "true")
                return true;
            if (data.ToLowerInvariant() == "false")
                return false;
            if (data == "1")
                return true;
            if (data == "0")
                return false;

            throw new Exception("Cannot Convert from \"" + data + "\" to Boolean");
        }
		
		public static DanbooruImage GetRandomImage(this IReadOnlyList<DanbooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];
		
		public static Rule34Image GetRandomImage(this IReadOnlyList<Rule34Image> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];
		
		public static RealbooruImage GetRandomImage(this IReadOnlyList<RealbooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static SafebooruImage GetRandomImage(this IReadOnlyList<SafebooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];
		
		public static GelbooruImage GetRandomImage(this IReadOnlyList<GelbooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static KonaChanImage GetRandomImage(this IReadOnlyList<KonaChanImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static E621Image GetRandomImage(this IReadOnlyList<E621Image> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static YandereImage GetRandomImage(this IReadOnlyList<YandereImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

        public static IGalleryItem GetRandomItem(this IEnumerable<IGalleryItem> list)
            => list.ElementAtOrDefault(Bot.services.GetRequiredService<Random>().Next(0,list.Count()));

        public static StoreScreenshotModel Random(this IReadOnlyList<StoreScreenshotModel> elements)
            => elements[Bot.services.GetRequiredService<Random>().Next(0, elements.Count)];

        public static IList<string> PaginateList(this IReadOnlyList<AnimeDataModel> list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for(int x=0;x<list.Count;x++)
            {
                var obj = list[x];

                pagetext += $"{x + 1}. {obj.Attributes.CanonicalTitle}\n";

                if((x+1) % 10 == 0 || (x + 1) == list.Count)
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

        public static IList<string> PaginatePosts(this Models.API.Reddit.Post[] posts, ITextChannel channel)
        {
            var Pages = new List<string>();

            string pagetext = "";
            
            for(int x=0;x<posts.Length;x++)
            {
                var post = posts[x];

                string txt = $"[{post.Data.Title}](https://reddit.com{post.Data.Permalink})\n";

                if (post.Data.Over18 && channel.IsNsfw)
                {
                    pagetext+="**NSFW** " + txt;
                }
                else
                {
                    pagetext += txt;
                }
                pagetext += "\n";

                if((x+1)%10 == 0 || (x + 1) == posts.Length)
                {
                    Pages.Add(pagetext);
                    pagetext = "";
                }
            }
            return Pages;
        }

        public static IList<string> PaginateList(this string[] list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count(); x++)
            {
                pagetext += $"{list[x]}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count())
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginateCodeBlockList(this string[] list)
        {
            var pages = new List<string>();
            string pagetext = "```cs\n";

            for (int x = 0; x < list.Count(); x++)
            {
                pagetext += $"{list[x]}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count())
                {
                    pages.Add("```");
                    pagetext = "";

                    if((x + 1) % 10 == 0)
                    {
                        pages.Add("```cs\n");
                    }
                }
            }

            return pages;
        }

        public static string ToString(this Embed embed)
        {
            string message = "";

            

            return message;
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
        
        public static MemoryStream ToMemoryStream(this string value)
            => new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));

        public static bool IsImageExtension(this string input)
        {
            foreach (var ext in ImageExtensions)
            {
                if (input.Contains(ext))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsVideoFile(this string input)
        {
            foreach(var x in VideoExtensions)
            {
                if (input.Contains(x) || input.EndsWith(x))
                    return true;
            }
            return false;
        }

        public static bool IsWebsite(this string input)
        {
            if (input.Contains('.') || input.Contains("www.") || 
                input.Contains("http://") || input.Contains("https://"))
            {
                return true;
            }

            return false;
        }

        public static bool IsValidOsuSig(this FileStream fs)
        {
            var header = new byte[4];

            fs.Read(header, 0, 4);

            var strHeader = Encoding.ASCII.GetString(header);
            return strHeader.ToLower().EndsWith("png");
        }

        public static string ToMessage(this Embed embed)
        {
            string message = "";
            if (embed.Author.HasValue)
                message += $"**__{embed.Author.Value.Name}__**\n";
            if (!String.IsNullOrEmpty(embed.Title))
                message += $"**{embed.Title}**\n";
            if (!String.IsNullOrEmpty(embed.Description))
                message += embed.Description + "\n";
            foreach (var field in embed.Fields)
                message += $"__{field.Name}__\n{field.Value}\n\n";
            if (embed.Video.HasValue)
                message += embed.Video.Value.Url + "\n";
            if (embed.Thumbnail.HasValue)
                message += embed.Thumbnail.Value.Url + "\n";
            if (embed.Image.HasValue)
                message += embed.Image.Value.Url + "\n";
            if (embed.Footer.HasValue)
                message += $"`{embed.Footer.Value.Text}`";
            if (embed.Timestamp.HasValue)
                message += " | " + embed.Timestamp.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt");
            return message;
        }

        public static async Task DeleteAfterSecondsAsync(this IUserMessage message, int timeout)
        {
            await Task.Delay((timeout * 1000));
            await message.DeleteAsync();
            await Bot.services.GetRequiredService<LoggingService>().AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Deleted a timed message", LogSeverity.Info));
        }

        public static string CheckEmptyWithLocale(this int? val, ResourceManager loc)
        {
            if(val.HasValue)
            {
                return Convert.ToString(val);
            }
            return loc.GetString("SKULD_GENERIC_EMPTY");
        }

        public static string CheckEmptyWithLocale(this string[] val, string seperator, ResourceManager loc)
        {
            if(val.Length == 0)
            {
                return loc.GetString("SKULD_GENERIC_EMPTY");
            }
            else
            {
                string msg = "";
                foreach(var item in val)
                {
                    var itm = item.CheckEmptyWithLocale(loc);
                    if (itm != loc.GetString("SKULD_GENERIC_EMPTY"))
                    {
                        msg += itm+seperator;
                    }
                }
                msg = msg.Remove(msg.Length - seperator.Length);
                return msg;
            }
        }

        public static string CheckEmptyWithLocale(this string val, ResourceManager loc)
            => val ?? loc.GetString("SKULD_GENERIC_EMPTY");

        public static Embed ToEmbed(this AnimeDataModel model, ResourceManager loc)
        {
            var attr = model.Attributes;

            var embed = new EmbedBuilder
            {
                Title = attr.CanonicalTitle.CheckEmptyWithLocale(loc)
            };
            
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SYNON"), attr.AbbreviatedTitles.CheckEmptyWithLocale(", ", loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_EPS"), attr.EpisodeCount.CheckEmptyWithLocale(loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SDATE"), attr.StartDate.CheckEmptyWithLocale(loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_EDATE"), attr.EndDate.CheckEmptyWithLocale(loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SCORE"), attr.RatingRank.CheckEmptyWithLocale(loc), true);

            if(attr.Synopsis.CheckEmptyWithLocale(loc) != loc.GetString("SKULD_GENERIC_EMPTY"))
            {
                var syno = attr.Synopsis;
                if (syno.Length > 1024)
                    embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis.Substring(0,1021) + "...", true);
                else
                    embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis, true);
            }

            embed.Color = Color.Purple;

            return embed.Build();
        }

        public static Embed ToEmbed(this MangaDataModel model, ResourceManager loc)
        {
            var attr = model.Attributes;

            var embed = new EmbedBuilder
            {
                Title = attr.CanonicalTitle.CheckEmptyWithLocale(loc)
            };

            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SYNON"), attr.AbbreviatedTitles.CheckEmptyWithLocale(", ", loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_EPS"), attr.ChapterCount.CheckEmptyWithLocale(loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SDATE"), attr.StartDate.CheckEmptyWithLocale(loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_EDATE"), attr.EndDate.CheckEmptyWithLocale(loc), true);
            embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SCORE"), attr.RatingRank.CheckEmptyWithLocale(loc), true);

            if (attr.Synopsis.CheckEmptyWithLocale(loc) != loc.GetString("SKULD_GENERIC_EMPTY"))
            {
                var syno = attr.Synopsis;
                if (syno.Length > 1024)
                    embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis.Substring(0, 1021) + "...", true);
                else
                    embed.AddField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP"), attr.Synopsis, true);
            }

            embed.Color = Color.Purple;

            return embed.Build();
        }

        public static string CheckForNull(this string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return null;
            else
                return s;
        }

        public static async Task<Embed> GetEmbedAsync(this Listing game)
        {
            var SteamStore = Bot.services.GetRequiredService<SteamWebAPI2.Interfaces.SteamStore>();
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
                Description = Tools.Tools.GetSteamGameDescription(game, app),
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
                Color = Tools.Tools.HexToDiscordColor(channel.ProfileBannerBackgroundColor)                
            };

            if(await channel.IsStreamingAsync())
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

                embed.ThumbnailUrl = channel.VideoBannerUrl;
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
            embed.AddField("Downvotes", word.DownVotes);
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

            var result = Bot.services.GetRequiredService<Random>().Next(0, 8193);
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
                    embed.AddField("Height", pokemon.Height + "mm", true);
                    embed.AddField("Weight", pokemon.Weight + "kg", true);
                    embed.AddField("ID", pokemon.ID.ToString(), true);
                    embed.AddField("Base Experience", pokemon.BaseExperience.ToString(), true);
                    break;

                case PokeSharpGroup.Abilities:
                    foreach (var ability in pokemon.Abilities)
                    {
                        embed.AddField(ability.Ability.Name, "Slot: " + ability.Slot, true);
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
                    embed.AddField("Game", games, true);
                    break;

                case PokeSharpGroup.HeldItems:
                    if (pokemon.HeldItems.Length > 0)
                    {
                        foreach (var hitem in pokemon.HeldItems)
                        {
                            foreach (var game in hitem.VersionDetails)
                            {
                                embed.AddField("Item", hitem.Item.Name + "\n**Game**\n" + game.Version.Name + "\n**Rarity**\n" + game.Rarity, true);
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
                        embed.AddField("Move", mve, true);
                    }
                    embed.Author.Url = "https://bulbapedia.bulbagarden.net/wiki/" + pokemon.Name + "_(Pokémon)";
                    embed.Footer = new EmbedFooterBuilder { Text = "Click the name to view more moves, I limited it to 4 to prevent a wall of text" };
                    break;

                case PokeSharpGroup.Stats:
                    foreach (var stat in pokemon.Stats)
                    {
                        embed.AddField(stat.Stat.Name, "Base Stat: " + stat.BaseStat, true);
                    }
                    break;
            }
            embed.ThumbnailUrl = sprite;

            return embed.Build();
        }
        
        public static async Task<IUserMessage> ReplyAsync(this ITextChannel channel, string message)
            => await ReplyAsync(channel, message);

        public static async Task<IUserMessage> ReplyAsync(this ITextChannel channel, string message, Embed embed)
            => await ReplyAsync(channel, message, embed);

        public static async Task<IUserMessage> ReplyAsync(this ITextChannel channel, Embed embed)
            => await ReplyAsync(channel, embed);

        public static async Task<IUserMessage> ReplyFailableAsync(this IDMChannel channel, string message)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<IUserMessage> ReplyFailableAsync(this IDMChannel channel, string message, Embed embed)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message, false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<IUserMessage> ReplyFailableAsync(this IDMChannel channel, Embed embed)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync("",false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<IUserMessage> ReplyAsync(this IDMChannel channel, ISocketMessageChannel backupchannel, string message)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return await ReplyAsync(backupchannel, "I couldn't send the message to your DMs, so I sent it here instead\n\n" + message);
            }
        }

        public static async Task<IUserMessage> ReplyAsync(this IDMChannel channel, ISocketMessageChannel backupchannel, string message, Embed embed)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync(message, false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return await ReplyAsync(backupchannel, "I couldn't send the message to your DMs, so I sent it here instead\n\n"+message, embed);
            }
        }

        public static async Task<IUserMessage> ReplyAsync(this IDMChannel channel, ISocketMessageChannel backupchannel, Embed embed)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                await channel.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                return await channel.SendMessageAsync("", false, embed);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return await ReplyAsync(backupchannel, "I couldn't send the message to your DMs, so I sent it here instead", embed);
            }
        }

        public static async Task<IUserMessage> ReplyAsync(this ISocketMessageChannel channel, string message)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<IUserMessage> ReplyAsync(this ISocketMessageChannel channel, string message, Embed embed)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                if (await textChan.CanEmbedAsync())
                {
                    return await mesgChan.SendMessageAsync(message, false, embed);
                }
                else
                {
                    return await mesgChan.SendMessageAsync(message + "\n\n" + embed.ToMessage());
                }
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<IUserMessage> ReplyAsync(this ISocketMessageChannel channel, Embed embed)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                if (await textChan.CanEmbedAsync())
                {
                    return await mesgChan.SendMessageAsync("", false, embed);
                }
                else
                {
                    return await mesgChan.SendMessageAsync(embed.ToMessage());
                }
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<IUserMessage> ReplyWithMentionAsync(this ISocketMessageChannel channel, IUser user, string message)
            => await ReplyAsync(channel, user.Mention + " " + message);

        public static async Task<IUserMessage> ReplyWithMentionAsync(this ISocketMessageChannel channel, IUser user, string message, Embed embed)
            => await ReplyAsync(channel, user.Mention + " " + message, embed);

        public static async Task<IUserMessage> ReplyWithMentionAsync(this ISocketMessageChannel channel, IUser user, Embed embed)
            => await ReplyAsync(channel, user.Mention, embed);

        public static async Task<IUserMessage> ReplyWithMentionAsync(this ITextChannel channel, IUser user, string message)
            => await ReplyAsync(channel, user.Mention + " " + message);

        public static async Task<IUserMessage> ReplyWithMentionAsync(this ITextChannel channel, IUser user, string message, Embed embed)
            => await ReplyAsync(channel, user.Mention + " " + message, embed);

        public static async Task<IUserMessage> ReplyWithMentionAsync(this ITextChannel channel, IUser user, Embed embed)
            => await ReplyAsync(channel, user.Mention, embed);

        public static async Task<IUserMessage> ReplyWithFileAsync(this ISocketMessageChannel channel, string filepath, string message = null)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return await mesgChan.SendFileAsync(filepath, message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<IUserMessage> ReplyWithFileAndMentionAsync(this ISocketMessageChannel channel, IUser user, string message = null)
        {
            if (message == null)
            {
                return await ReplyWithFileAsync(channel, user.Mention);
            }
            else
            {
                return await ReplyWithFileAsync(channel, user.Mention + " " + message);
            }
        }

        public static async Task ReplyWithTimedMessage(this ISocketMessageChannel channel, string message, double timeout)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                var msg = await mesgChan.SendMessageAsync(message);
                await msg.DeleteAfterSecondsAsync((int)timeout);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return;
            }
        }

        public static async Task ReplyWithTimedMessage(this ITextChannel channel, string message, double timeout)
        {
            var logger = Bot.services.GetRequiredService<LoggingService>();
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                var msg = await mesgChan.SendMessageAsync(message);
                await msg.DeleteAfterSecondsAsync((int)timeout);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return;
            }
        }

        public static async Task<bool> CanEmbedAsync(this ITextChannel channel)
            => (await channel.Guild.GetCurrentUserAsync()).GetPermissions(channel).EmbedLinks;

        public static string ToText(this Embed embed)
        {
            string message = "";
            if (embed.Author.HasValue)
                message += $"**__{embed.Author.Value.Name}__**\n";
            if (!String.IsNullOrEmpty(embed.Title))
                message += $"**{embed.Title}**\n";
            if (!String.IsNullOrEmpty(embed.Description))
                message += embed.Description + "\n";
            foreach (var field in embed.Fields)
                message += $"__{field.Name}__\n{field.Value}\n\n";
            if (embed.Video.HasValue)
                message += embed.Video.Value.Url + "\n";
            if (embed.Thumbnail.HasValue)
                message += embed.Thumbnail.Value.Url + "\n";
            if (embed.Image.HasValue)
                message += embed.Image.Value.Url + "\n";
            if (embed.Footer.HasValue)
                message += $"`{embed.Footer.Value.Text}`";
            if (embed.Timestamp.HasValue)
                message += " | " + embed.Timestamp.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt");
            return message;
        }

        //https://gist.github.com/frankhale/3240804
        public static string StripHtml(this string value)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);

            if (htmlDoc == null)
                return value;

            StringBuilder sanitizedString = new StringBuilder();

            foreach (var node in htmlDoc.DocumentNode.ChildNodes)
                sanitizedString.Append(node.InnerText);

            return sanitizedString.ToString();
        }
    }
}
