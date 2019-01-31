using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using PokeSharp;
using Skuld.APIS;
using Skuld.APIS.Extensions;
using Skuld.APIS.Pokemon.Models;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord;
using Skuld.Discord.Attributes;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using SteamStoreQuery;
using SteamWebAPI2.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Search : InteractiveBase<SkuldCommandContext>
    {
        public SocialAPIS Social { get; set; }
        public Random Random { get; set; }
        public PokeSharpClient PokeMonClient { get; set; }
        public SteamStore SteamStoreClient { get; set; }
        public UrbanDictionaryClient UrbanDictionary { get; set; }
        public WikipediaClient Wikipedia { get; set; }
        public GiphyClient Giphy { get; set; }
        public Stands4Client Stands4 { get; set; }
        public BaseClient WebHandler { get; set; }
        public SkuldConfig Configuration { get; set; }

        [Command("twitch"), Summary("Finds a twitch user"), RequireTwitch]
        public async Task TwitchSearch([Remainder]string twitchStreamer)
        {
            var channel = await TwitchClient.GetUserAsync(twitchStreamer);

            await (await channel.GetEmbedAsync()).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("instagram"), Alias("insta"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Instagram(string usr)
        {
            var data = await Social.GetInstagramUserAsync(usr);
            if (data != null)
            {
                if (!data.PrivateAccount)
                {
                    if (data.TimelineMedia.Images.Count() > 0)
                    {
                        var post = data.TimelineMedia.Images.FirstOrDefault().Node;

                        var embed = new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder
                            {
                                Name = $"{data.FullName} ({data.Username})",
                                IconUrl = data.ProfilePicture,
                                Url = "https://instagr.am/" + data.Username
                            },
                            ImageUrl = post.DisplaySrc,
                            Description = post.PrimaryCaption,
                            Title = "Recent Post",
                            Url = "https://www.instagr.am/p/" + post.Code + "/",
                            Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(post.Date),
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "Uploaded"
                            },
                            Color = EmbedUtils.RandomColor()
                        };
                        await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                        await "This account has no images in their feed".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                    await "This account is a Private Account, so I can't access their feed.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
                await $"I can't find an account named: `{usr}`. Check your spelling and try again.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("steam"), Summary("Searches steam store")]
        public async Task SteamStore([Remainder]string game)
        {
            var steam = Query.Search(game);

            if (steam.Capacity > 0)
            {
                if (steam.Count > 1)
                {
                    var Pages = steam.PaginateList();

                    if (Pages.Count > 1)
                    {
                        await PagedReplyAsync(new PaginatedMessage
                        {
                            Pages = Pages,
                            Title = "Type the number of what you want",
                            Color = Color.Purple
                        });
                    }
                    else
                    {
                        await $"Type the number of what you want:\n{string.Join("\n", Pages)}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }

                    var message = await NextMessageAsync();

                    int.TryParse(message.Content, out int selectedapp);
                    if (selectedapp <= 0)
                    {
                        await "Incorrect input".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                        return;
                    }

                    selectedapp--;

                    await (await steam[selectedapp].GetEmbedAsync()).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    await (await steam[0].GetEmbedAsync()).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
            }
            else
            {
                await "I found nothing from steam".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("search"), Summary("Use \"g\" as a short cut for google,\n\"yt\" for youtube,\nor search for images on imgur"), Alias("s")]
        public async Task GetSearch(string platform, [Remainder]string query)
        {
            platform = platform.ToLowerInvariant();
            if (platform == "google" || platform == "g")
            {
                await $"🔍 Searching Google for: {query}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                var result = await SearchClient.SearchGoogleAsync(query);
                await result.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            if (platform == "youtube" || platform == "yt")
            {
                await $"🔍 Searching Youtube for: {query}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                var type = Context.Channel.EnterTypingState();
                var result = await SearchClient.SearchYoutubeAsync(query);
                await result.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            if (platform == "imgur")
            {
                await "🔍 Searching Imgur for: {query}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                var result = await SearchClient.SearchImgurAsync(query);
                await result.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("lmgtfy"), Summary("Creates a \"lmgtfy\"(Let me google that for you) link")]
        public async Task LMGTFY(string engine, [Remainder]string query)
        {
            string url = "https://lmgtfy.com/";
            engine = engine.ToLowerInvariant();
            if (engine == "g" || engine == "google")
            { url = url + "?q=" + query.Replace(" ", "%20"); }
            if (engine == "b" || engine == "bing")
            { url = url + "?s=b&q=" + query.Replace(" ", "%20"); }
            if (engine == "y" || engine == "yahoo")
            { url = url + "?s=y&q=" + query.Replace(" ", "%20"); }
            if (engine == "a" || engine == "aol")
            { url = url + "?a=b&q=" + query.Replace(" ", "%20"); }
            if (engine == "k" || engine == "ask")
            { url = url + "?k=b&q=" + query.Replace(" ", "%20"); }
            if (engine == "d" || engine == "duckduckgo")
            { url = url + "?s=d&q=" + query.Replace(" ", "%20"); }
            if (url != "https://lmgtfy.com/")
                await url.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else
            {
                await new EmbedBuilder { Author = new EmbedAuthorBuilder { Name = "Error with command" }, Color = Color.Red, Description = $"Ensure your parameters are correct, example: `{Configuration.Discord.Prefix}lmgtfy g How to use lmgtfy`" }.Build().QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new[] { "generic" });
            }
        }

        [Command("urban"), Summary("Gets a thing from urban dictionary if empty, it gets a random thing"), RequireNsfw]
        public async Task Urban([Remainder]string phrase = null)
        {
            if (phrase == null)
                await (await UrbanDictionary.GetRandomWordAsync()).ToEmbed().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else
                await (await UrbanDictionary.GetPhraseAsync(phrase)).ToEmbed().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("wikipedia"), Summary("Gets wikipedia information, supports all languages that wikipedia offers"), Alias("wiki")]
        public async Task Wiki(string langcode, [Remainder]string query)
        {
            var page = await Wikipedia.GetArticleAsync(langcode, query);
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = page.Name,
                    Url = page.Url
                },
                Color = EmbedUtils.RandomColor()
            };
            embed.AddField("Description", page.Description ?? "Not Available", true);
            await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("define"), Summary("Defines a word")]
        public async Task Define([Remainder]string word)
        {
            var definedword = await Stands4.GetWordAsync(word);

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = definedword.Word
                },
                Color = EmbedUtils.RandomColor()
            };
            embed.AddField("Definition", definedword.Definition ?? "None Available");
            embed.AddField("Example", definedword.Example ?? "Not Available");
            embed.AddField("Part of speech", definedword.PartOfSpeech ?? "Not Available", inline: true);
            embed.AddField("Terms", definedword.Terms ?? "Not Available", inline: true);

            await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("reddit"), Summary("Gets a subreddit")]
        public async Task SubReddit(string subreddit)
        {
            var channel = (ITextChannel)Context.Channel;

            var subReddit = await Social.GetSubRedditAsync(subreddit);
            var paginatedMessage = new PaginatedMessage
            {
                Title = "https://reddit.com/" + subreddit,
                Color = Color.Blue,
                Options = new PaginatedAppearanceOptions
                {
                    DisplayInformationIcon = false,
                    JumpDisplayOptions = JumpDisplayOptions.WithManageMessages
                }
            };

            paginatedMessage.Pages = subReddit.Data.Posts.PaginatePosts(channel);

            if (paginatedMessage.Pages.Count() > 1)
            {
                await PagedReplyAsync(paginatedMessage);
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Title = paginatedMessage.Title,
                    Description = paginatedMessage.Pages.FirstOrDefault().ToString(),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Page 1/1"
                    },
                    Color = Color.Blue
                };

                await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("pokemon"), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(string pokemon, string group = null)
            => await SendPokemonAsync(await PokeMonClient.GetPocketMonsterAsync(pokemon.ToLowerInvariant()), group ?? "default").ConfigureAwait(false);

        [Command("pokemon"), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(int pokemonid, string group = null)
            => await SendPokemonAsync(await PokeMonClient.GetPocketMonsterAsync(pokemonid), group ?? "default").ConfigureAwait(false);

        public async Task SendPokemonAsync(PokeSharp.Models.PocketMonster pokemon, string group)
        {
            if (pokemon == null)
            {
                await new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Command Error!",
                    Description = "This pokemon doesn't exist. Please try again.\nIf it is a Generation 7, pokeapi.co hasn't updated for it yet."
                }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
            }
            else
            {
                Embed embed = null;

                group = group.ToLower();

                if (group == "stat" || group == "stats")
                {
                    embed = pokemon.GetEmbed(PokeSharpGroup.Stats);
                }
                if (group == "abilities" || group == "ability")
                {
                    embed = pokemon.GetEmbed(PokeSharpGroup.Abilities);
                }
                if (group == "helditems" || group == "hitems" || group == "hitem" || group == "items")
                {
                    embed = pokemon.GetEmbed(PokeSharpGroup.HeldItems);
                }
                if (group == "default")
                {
                    embed = pokemon.GetEmbed(PokeSharpGroup.Default);
                }
                if (group == "move" || group == "moves")
                {
                    embed = pokemon.GetEmbed(PokeSharpGroup.Moves);
                }
                if (group == "games" || group == "game")
                {
                    embed = pokemon.GetEmbed(PokeSharpGroup.Games);
                }
                await embed.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("osu!"), Summary("Get a person's Osu! Sig")]
        public async Task OsuSig([Remainder]string User) => await SendSigAsync(0, User);

        [Command("osu!taiko"), Summary("Gets a person's Osu!Taiko Sig")]
        public async Task TaikoSig([Remainder]string User) => await SendSigAsync(1, User);

        [Command("osu!ctb"), Summary("Gets a person's Osu!CTB Sig")]
        public async Task CTBSig([Remainder]string User) => await SendSigAsync(2, User);

        [Command("osu!mania"), Summary("Gets a person's Osu!Mania Sig")]
        public async Task ManiaSig([Remainder]string User) => await SendSigAsync(3, User);

        private async Task SendSigAsync(int mode, string User)
        {
            Uri url = null;

            var folder = AppContext.BaseDirectory + "/storage/OsuSigs/";

            string msgmode = "";

            if (mode == 0)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "Standard/";
            }
            if (mode == 1)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&mode=1&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "Taiko/";
                msgmode = "Taiko";
            }
            if (mode == 2)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&mode=2&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "CTB/";
                msgmode = "CTB";
            }
            if (mode == 3)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&mode=3&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "Mania/";
                msgmode = "Mania";
            }

            if (!Directory.Exists(folder))
            { Directory.CreateDirectory(folder); }

            var filepath = folder + User + ".png";
            await WebHandler.DownloadFileAsync(url, filepath);

            var file = File.OpenRead(filepath);

            if (file.IsValidOsuSig())
            {
                await "".QueueMessage(Discord.Models.MessageType.File, Context.User, Context.Channel, filepath);
            }
            else
            {
                await $"The user either doesn't exist or they haven't played osu!{msgmode}".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }

            file.Close();

            File.Delete(filepath);
        }
    }
}