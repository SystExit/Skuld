using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;
using Skuld.Models.API;
using Skuld.APIS;
using Skuld.Tools;
using System.IO;
using Discord.Addons.Interactive;
using Skuld.Services;
using PokeSharp;
using SteamStoreQuery;
using SteamWebAPI2.Interfaces;
using Skuld.Extensions;
using Skuld.Models;
using Skuld.Utilities;

namespace Skuld.Modules
{
    [Group]
    public class Search : InteractiveBase<ShardedCommandContext>
    {
        public TwitchService Twitch { get; set; }
        public SocialAPIS Social { get; set; }
        public LoggingService Logger { get; set; }
        public Random Random { get; set; }
        public PokeSharpClient PokeMonClient { get; set; }
        public SteamStore SteamStoreClient { get; set; }
        public SearchService SearchClient { get; set; }

        [Command("twitch"), Summary("Finds a twitch user")]
        public async Task TwitchSearch([Remainder]string twitchStreamer)
        {
            var channel = await Twitch.GetUserAsync(twitchStreamer);

            await Context.Channel.ReplyAsync(await channel.GetEmbedAsync());
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
                        Models.API.Social.Instagram.Image post = data.TimelineMedia.Images.FirstOrDefault().Node;

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
                        await Context.Channel.ReplyAsync(embed.Build());
                    }
                    else
                        await Context.Channel.ReplyAsync("This account has no images in their feed");
                }
                else
                    await Context.Channel.ReplyAsync("This account is a Private Account, so I can't access their feed.");
            }
            else
                await Context.Channel.ReplyAsync($"I can't find an account named: `{usr}`. Check your spelling and try again.");
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
                        await Context.Channel.ReplyAsync("Type the number of what you want:\n" + String.Join("\n", Pages));
                    }

                    var message = await NextMessageAsync();

                    int.TryParse(message.Content, out int selectedapp);
                    if (selectedapp <= 0)
                    {
                        await Context.Channel.ReplyAsync("Incorrect input");
                        return;
                    }

                    selectedapp--;

                    await Context.Channel.ReplyAsync(await steam[selectedapp].GetEmbedAsync());
                }
                else
                {
                    await Context.Channel.ReplyAsync(await steam[0].GetEmbedAsync());
                }
            }
            else
            {
                await Context.Channel.ReplyAsync("I found nothing from steam");
            }
        }

        [Command("search"), Summary("Use \"g\" as a short cut for google,\n\"yt\" for youtube,\nor search for images on imgur"), Alias("s")]
        public async Task GetSearch(string platform, [Remainder]string query)
        {
            platform = platform.ToLowerInvariant();
            if (platform == "google" || platform == "g")
            {
                var result = await SearchClient.SearchGoogleAsync(query);
                await Context.Channel.ReplyAsync(result);
            }
            if (platform == "youtube" || platform == "yt")
            {
                var result = await SearchClient.SearchYoutubeAsync(query);
                await Context.Channel.ReplyAsync(result);
            }
            if (platform == "imgur")
            {
                var result = await SearchClient.SearchImgurAsync(query);
                await Context.Channel.ReplyAsync(result);
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
                await Context.Channel.ReplyAsync(url);
            else
            {
                await Context.Channel.ReplyAsync(new EmbedBuilder { Author = new EmbedAuthorBuilder { Name = "Error with command" }, Color = Color.Red, Description = $"Ensure your parameters are correct, example: `{Bot.Configuration.Discord.Prefix}lmgtfy g How to use lmgtfy`" }.Build());
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new[] { "generic" });
            }
        }

        [Command("urban"), Summary("Gets a thing from urban dictionary if empty, it gets a random thing")]
        public async Task Urban([Remainder]string phrase = null)
        {
            if (phrase == null)
                await Context.Channel.ReplyAsync((await UrbanDictionary.GetRandomWordAsync()).ToEmbed());
            else
                await Context.Channel.ReplyAsync((await UrbanDictionary.GetPhraseAsync(phrase)).ToEmbed());
        }

        [Command("wikipedia"), Summary("Gets wikipedia information, supports all languages that wikipedia offers"), Alias("wiki")]
        public async Task Wiki(string langcode, [Remainder]string query)
        {
            var jsonresp = JObject.Parse((await WebHandler.ReturnStringAsync(new Uri($"https://{langcode}.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={query}"))));
            dynamic item = jsonresp["query"]["pages"].First.First;
            string desc = Convert.ToString(item["extract"]);
            var page = new Wiki()
            {
                Name = item["title"].ToString(),
                Description = desc.Remove(500) + "...\nRead more at the article.",
                Url = $"https://{langcode}.wikipedia.org/wiki/{query}"
            };
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
            await Context.Channel.ReplyAsync(embed.Build());
        }

        [Command("gif"), Summary("Gets a gif")]
        public async Task Gifcommand([Remainder]string query)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Giphy",
                    IconUrl = "https://giphy.com/favicon.ico",
                    Url = "https://giphy.com/"
                },
                Color = EmbedUtils.RandomColor()
            };
            try
            {
                query = query.Replace(" ", "%20");
                var rawresp = await WebHandler.ReturnStringAsync(new Uri($"https://api.giphy.com/v1/gifs/search?q={query}&api_key=dc6zaTOxFJmzC"));
                var jsonresp = JObject.Parse(rawresp);
                var photo = (JArray)jsonresp["data"];
                dynamic item = photo[Random.Next(0, photo.Count)];
                var gif = new Gif(item["id"].ToString());
                embed.Url = gif.Url;
                embed.ImageUrl = gif.Url;
                await Context.Channel.ReplyAsync(embed.Build());
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                { await Context.Channel.ReplyAsync(Context.User.Mention + " No results found for: `" + query + "`"); }
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
            }
        }

        [Command("define"), Summary("Defines a word")]
        public async Task Define([Remainder]string word)
        {
            var stringifiedxml = await WebHandler.ReturnStringAsync(new Uri($"http://www.stands4.com/services/v2/defs.php?uid={Bot.Configuration.APIS.STANDSUid}&tokenid={Bot.Configuration.APIS.STANDSToken}&word={word}"));
            var xml = new XmlDocument();
            xml.LoadXml(stringifiedxml);
            XObject xNode = XDocument.Parse(xml.InnerXml);
            var jobject = JObject.Parse(JsonConvert.SerializeXNode(xNode));
            dynamic item = jobject["results"]["result"].First();
            var definedword = new Define(word, item["definition"].ToString(), item["example"].ToString(), item["partofspeech"].ToString(), item["term"].ToString());
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
            await Context.Channel.ReplyAsync(embed.Build());
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

                await Context.Channel.ReplyAsync(embed.Build());
            }
        }

        [Command("pokemon"), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(string pokemon, string group = null) =>
    await SendPokemonAsync(await PokeMonClient.GetPocketMonsterAsync(pokemon.ToLowerInvariant()), group ?? "default").ConfigureAwait(false);
        [Command("pokemon"), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(int pokemonid, string group = null) =>
            await SendPokemonAsync(await PokeMonClient.GetPocketMonsterAsync(pokemonid), group ?? "default").ConfigureAwait(false);

        public async Task SendPokemonAsync(PokeSharp.Models.PocketMonster pokemon, string group)
        {
            if (pokemon == null)
            {
                await Context.Channel.ReplyAsync(new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Command Error!",
                    Description = "This pokemon doesn't exist. Please try again.\nIf it is a Generation 7, pokeapi.co hasn't updated for it yet."
                }.Build());
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
                await Context.Channel.ReplyAsync(embed);
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

            var folder = AppContext.BaseDirectory + "/skuld/storage/OsuSigs/";

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
            { await Context.Channel.SendFileAsync(filepath); }
            else
            {
                await Context.Channel.ReplyAsync("The user either doesn't exist or they haven't played osu!" + msgmode);
            }

            file.Close();

            File.Delete(filepath);
        }
    }
}
